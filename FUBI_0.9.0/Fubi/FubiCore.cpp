// ****************************************************************************************
//
// Fubi FubiCore
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************

#include "FubiCore.h"

// Defines for enabling/disabling OpenNI and OpenCV dependencies
#include "FubiConfig.h"

// Posture recognition
#include "FubiRecognizerFactory.h"

// Image processing
#include "FubiImageProcessing.h"

#ifdef FUBI_USE_OPENNI2
// OpenNI v2.x integration
#include "FubiOpenNI2Sensor.h"
#endif
#ifdef FUBI_USE_OPENNI1
// OpenNI v1.x integration
#include "FubiOpenNISensor.h"
#endif
#ifdef FUBI_USE_KINECT_SDK
// Kinect SDK integration
#include "FubiKinectSDKSensor.h"
#endif

#ifdef FUBI_USE_LEAP
// Leap sensor integration
#include "FubiLeapSensor.h"
#endif

// Xml parsing
#include "FubiXMLParser.h"


// Sorting and more
#include <algorithm>

using namespace Fubi;
using namespace std;

const std::string FubiCore::s_emtpyString;

FubiCore* FubiCore::s_instance = 0x0;

FubiCore::~FubiCore()
{
	for (unsigned int i = 0; i < Postures::NUM_POSTURES; ++i)
	{
		delete m_postureRecognizers[i];
		m_postureRecognizers[i] = 0x0;
	}
	clearUserDefinedRecognizers();

	for (unsigned int i = 0; i < MaxUsers; ++i)
	{
		delete m_users[i];
		m_users[i] = 0x0;
	}
	m_numUsers = 0;

	for (unsigned int i = 0; i < MaxHands; ++i)
	{
		delete m_hands[i];
		m_hands[i] = 0x0;
	}
	m_numHands = 0;

	delete m_sensor; m_sensor = 0x0;
	delete m_fingerSensor; m_fingerSensor = 0x0;
}

FubiCore::FubiCore() : m_numUsers(0), m_numHands(0), m_sensor(0x0), m_fingerSensor(0x0)
{

	for (unsigned int i = 0; i < MaxUsers; ++i)
	{
		m_users[i] = new FubiUser();
	}

	for (unsigned int i = 0; i < MaxHands; ++i)
	{
		m_hands[i] = new FubiHand();
	}

	for (unsigned int i = 0; i < Combinations::NUM_COMBINATIONS+1; ++i)
	{
		m_autoStartCombinationRecognizers[i] = false;
	}

	// Init posture recognizers
	for (unsigned int i = 0; i < Postures::NUM_POSTURES; ++i)
	{
		m_postureRecognizers[i] = createPostureRecognizer((Postures::Posture)i);
	}
}

bool FubiCore::initFromXml(const char* xmlPath, Fubi::SkeletonTrackingProfile::Profile profile /*= Fubi::SkeletonTrackingProfile::ALL*/,
	bool mirrorStream /*= true*/, bool registerStreams /*=true*/)
{
	delete m_sensor;
	m_sensor = 0x0;

	for (unsigned int i = 0; i < MaxUsers; ++i)
	{
		m_users[i]->reset();
	}
	m_numUsers = 0;
	m_userIDToUsers.clear();

	if (xmlPath != 0)
	{
#ifdef FUBI_USE_OPENNI1
		m_sensor = new FubiOpenNISensor();
		if (!m_sensor->initFromXml(xmlPath, profile, mirrorStream, registerStreams))
		{
			delete m_sensor;
			m_sensor = 0x0;
			return false;
		}
#else	
		Fubi_logErr("Tried to init OpenNI 1.x via XML, but no activated sensor type supports xml init.\n -Did you forget to uncomment the FUBI_USE_OPENNI1 define in the FubiConfig.h?\n");
		return false;
#endif
	}
	return true;
}

bool FubiCore::initSensorWithOptions(const Fubi::SensorOptions& options)
{
	delete m_sensor;
	m_sensor = 0x0;
	bool success = false;

	for (unsigned int i = 0; i < MaxUsers; ++i)
	{
		m_users[i]->reset();
	}
	m_numUsers = 0;
	m_userIDToUsers.clear();

	if (options.m_type == SensorType::OPENNI2)
	{
#ifdef FUBI_USE_OPENNI2
		m_sensor = new FubiOpenNI2Sensor();
		success = m_sensor->initWithOptions(options);
#else
		Fubi_logErr("Openni 2.x sensor is not activated\n -Did you forget to uncomment the FUBI_USE_OPENNI2 define in the FubiConfig.h?\n");	
#endif
	}
	else if (options.m_type == SensorType::OPENNI1)
	{
#ifdef FUBI_USE_OPENNI1
		m_sensor = new FubiOpenNISensor();
		success = m_sensor->initWithOptions(options);
#else
		Fubi_logErr("Openni 1.x sensor is not activated\n -Did you forget to uncomment the FUBI_USE_OPENNI1 define in the FubiConfig.h?\n");	
#endif
	}
	else if (options.m_type == SensorType::KINECTSDK)
	{
#ifdef FUBI_USE_KINECT_SDK
		m_sensor = new FubiKinectSDKSensor();
		success = m_sensor->initWithOptions(options);
#else
		Fubi_logErr("Kinect SDK sensor is not activated\n -Did you forget to uncomment the USE_OPENNIX/USE_KINECTSDK define in the FubiConfig.h?\n");	
#endif
	}
	else if (options.m_type == SensorType::NONE)
	{
		Fubi_logInfo("FubiCore: Current sensor deactivated, now in non-tracking mode!\n");
		success = true;
	}

	if (!success)
	{
		delete m_sensor;
		m_sensor = 0x0;
	}

	return success;
}

void FubiCore::updateSensor()
{
	if (m_sensor)
	{
		m_sensor->update();

		// Get the current number and ids of users, adapt the useridTouser map
		// init new users and let them do the update for tracking info
		updateUsers();
	}

	if (m_fingerSensor)
	{
		// Update sensor data
		m_fingerSensor->update();

		// Get all tracked hands, adapt the handIdToUser map, init new hands
		// 
		updateHands();
	}
}


Fubi::RecognitionResult::Result FubiCore::recognizeGestureOn(Postures::Posture postureID, unsigned int userID)
{
	if (postureID < Postures::NUM_POSTURES && m_postureRecognizers[postureID])
	{
		FubiUser* user = getUser(userID);
		if (user)
		{	
			// Found user
			return m_postureRecognizers[postureID]->recognizeOn(user);
		}
	}
	return Fubi::RecognitionResult::NOT_RECOGNIZED;
}

Fubi::RecognitionResult::Result FubiCore::recognizeGestureOn(unsigned int recognizerIndex, unsigned int userID, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	if (recognizerIndex < m_userDefinedRecognizers.size()
		&& (m_userDefinedRecognizers[recognizerIndex].second->m_targetSensor == RecognizerTarget::ALL_SENSORS || m_userDefinedRecognizers[recognizerIndex].second->m_targetSensor == RecognizerTarget::BODY_SENSOR))
	{
		FubiUser* user = getUser(userID);
		if (user)
		{
			// Found the user
			if (user->m_isTracked && user->m_inScene)
				return m_userDefinedRecognizers[recognizerIndex].second->recognizeOn(user, correctionHint);
			else
				return Fubi::RecognitionResult::TRACKING_ERROR;
		}
	}
	return Fubi::RecognitionResult::NOT_RECOGNIZED;
}

Fubi::RecognitionResult::Result FubiCore::recognizeGestureOn(const string& name, unsigned int userID, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	int recognizerIndex = getUserDefinedRecognizerIndex(name);
	if (recognizerIndex >= 0)
		return recognizeGestureOn((unsigned) recognizerIndex, userID, correctionHint);
	return Fubi::RecognitionResult::NOT_RECOGNIZED;
}

Fubi::RecognitionResult::Result FubiCore::recognizeGestureOnHand(const string& name, unsigned int handID)
{
	int recognizerIndex = getUserDefinedRecognizerIndex(name);
	if (recognizerIndex >= 0
		&& (m_userDefinedRecognizers[recognizerIndex].second->m_targetSensor == RecognizerTarget::ALL_SENSORS || m_userDefinedRecognizers[recognizerIndex].second->m_targetSensor == RecognizerTarget::FINGER_SENSOR))
	{
		FubiHand* hand = getHand(handID);
		if (hand && hand->m_isTracked)
		{
			// Found the hand
			return m_userDefinedRecognizers[recognizerIndex].second->recognizeOn(hand);
		}
	}
	return Fubi::RecognitionResult::NOT_RECOGNIZED;
}

int FubiCore::getUserDefinedRecognizerIndex(const std::string& name)
{
	if (name.length() > 0)
	{
		vector<pair<string, IGestureRecognizer*> >::iterator iter = m_userDefinedRecognizers.begin();
		vector<pair<string, IGestureRecognizer*> >::iterator end = m_userDefinedRecognizers.end();
		for (int i = 0; iter != end; ++iter, ++i)
		{
			if (name == iter->first)
				return i;
		}
	}
	return -1;
}

int FubiCore::getHiddenUserDefinedRecognizerIndex(const std::string& name)
{
	if (name.length() > 0)
	{
        vector<pair<string, IGestureRecognizer*> >::iterator iter = m_hiddenUserDefinedRecognizers.begin();
        vector<pair<string, IGestureRecognizer*> >::iterator end = m_hiddenUserDefinedRecognizers.end();
		for (int i = 0; iter != end; ++iter, ++i)
		{
			if (name == iter->first)
				return i;
		}
	}
	return -1;
}

int FubiCore::getUserDefinedCombinationRecognizerIndex(const std::string& name)
{
	if (name.length() > 0)
	{
        vector<pair<string, CombinationRecognizer*> >::iterator iter = m_userDefinedCombinationRecognizers.begin();
        vector<pair<string, CombinationRecognizer*> >::iterator end = m_userDefinedCombinationRecognizers.end();
		for (int i = 0; iter != end; ++iter, ++i)
		{
			if (name == iter->first)
				return i;
		}
	}
	return -1;
}

CombinationRecognizer* FubiCore::getUserDefinedCombinationRecognizer(unsigned int index)
{
	if (index < m_userDefinedCombinationRecognizers.size())
	{
		return m_userDefinedCombinationRecognizers[index].second;
	}
	return 0x0;
}

CombinationRecognizer* FubiCore::getUserDefinedCombinationRecognizer(const std::string& name)
{
	if (name.length() > 0)
	{
        vector<pair<string, CombinationRecognizer*> >::iterator iter = m_userDefinedCombinationRecognizers.begin();
        vector<pair<string, CombinationRecognizer*> >::iterator end = m_userDefinedCombinationRecognizers.end();
		for (int i = 0; iter != end; ++iter, ++i)
		{
			if (name == iter->first)
				return iter->second;
		}
	}
	return 0x0;
}

void FubiCore::enableCombinationRecognition(Combinations::Combination combinationID, unsigned int userID, bool enable)
{
	// Standard case: enable/disable a single recognizer
	if (combinationID < Combinations::NUM_COMBINATIONS)
	{
		FubiUser* user = getUser(userID);
		if (user)
		{	
			// Found user
			user->enableCombinationRecognition(combinationID, enable);
		}
	}
	// Special case: enable/disable all recognizers (even the user defined ones!)
	else if (combinationID == Combinations::NUM_COMBINATIONS)
	{
		FubiUser* user = getUser(userID);
		if (user)
		{
			for (unsigned int i = 0; i < Combinations::NUM_COMBINATIONS; ++i)
				user->enableCombinationRecognition((Combinations::Combination)i, enable);

            std::vector<std::pair<std::string, CombinationRecognizer*> >::iterator iter;
            std::vector<std::pair<std::string, CombinationRecognizer*> >::iterator end = m_userDefinedCombinationRecognizers.end();
			for (iter = m_userDefinedCombinationRecognizers.begin(); iter != end; ++iter)
			{
				user->enableCombinationRecognition(iter->second, enable);
			}
		}
	}
}

void FubiCore::enableCombinationRecognition(const std::string& combinationName, unsigned int userID, bool enable)
{
	FubiUser* user = getUser(userID);
	if (user)
	{	
		// Found user
		user->enableCombinationRecognition(getUserDefinedCombinationRecognizer(combinationName), enable);
	}
}

void FubiCore::enableCombinationRecognitionHand(const std::string& combinationName, unsigned int handID, bool enable)
{
	FubiHand* hand = getHand(handID);
	if (hand)
	{	
		// Found hand
		hand->enableCombinationRecognition(getUserDefinedCombinationRecognizer(combinationName), enable);
	}
}

bool FubiCore::getAutoStartCombinationRecognition(Fubi::Combinations::Combination combinationID /*= Fubi::Combinations::NUM_COMBINATIONS*/)
{
	if (m_autoStartCombinationRecognizers[Fubi::Combinations::NUM_COMBINATIONS])
		return true;
	return m_autoStartCombinationRecognizers[combinationID];
}

void FubiCore::setAutoStartCombinationRecognition(bool enable, Combinations::Combination combinationID /*= Combinations::NUM_COMBINATIONS*/)
{
	if (combinationID < Combinations::NUM_COMBINATIONS)
	{
		m_autoStartCombinationRecognizers[combinationID] = enable;
		if (enable)
		{
			// Enable it for all current users
			for (unsigned int user = 0; user < m_numUsers; user++)
			{
				m_users[user]->enableCombinationRecognition(combinationID, true);
			}
		}
	}
	else if (combinationID == Combinations::NUM_COMBINATIONS)
	{
		for (unsigned int i = 0; i < Combinations::NUM_COMBINATIONS; ++i)
			setAutoStartCombinationRecognition(enable, (Combinations::Combination)i);

		m_autoStartCombinationRecognizers[Combinations::NUM_COMBINATIONS] = enable;
		if (enable)
		{
			// Enable user defined recognizers for all current users
			for (unsigned int user = 0; user < m_numUsers; user++)
			{
				std::vector<std::pair<std::string, CombinationRecognizer*> >::iterator iter;
				std::vector<std::pair<std::string, CombinationRecognizer*> >::iterator end = m_userDefinedCombinationRecognizers.end();
				for (iter = m_userDefinedCombinationRecognizers.begin(); iter != end; ++iter)
				{
					m_users[user]->enableCombinationRecognition(iter->second, true);
				}
			}
		}
	}
}

FubiUser* FubiCore::getUser(unsigned int userId)
{
	map<unsigned int, FubiUser*>::const_iterator iter = m_userIDToUsers.find(userId);
	if (iter != m_userIDToUsers.end())
	{
		return iter->second;
	}
	return 0;
}

FubiHand* FubiCore::getHand(unsigned int handID)
{
	map<unsigned int, FubiHand*>::const_iterator iter = m_handIDToHands.find(handID);
	if (iter != m_handIDToHands.end())
	{
		return iter->second;
	}
	return 0;
}

void FubiCore::updateHands()
{
	static unsigned int handIDs[MaxHands];

	if (m_fingerSensor)
	{
		m_numHands = m_fingerSensor->getHandIDs(handIDs);

		// First sort our hand array according to the given id array
		for (unsigned int i = 0; i < m_numHands; ++i)
		{
			unsigned int id = handIDs[i];
			FubiHand* hand = m_hands[i];
			if (hand->m_id != id) // hand at the wrong place or still unknown
			{
				// Try to find the hand with the correct id (can only be later in the array as we already have corrected the ones before)
				// or at least find a free slot to move the hand that is currently here to
				unsigned int oldIndex = -1;
				unsigned int firstFreeIndex = -1;
				for (unsigned int j = i; j < MaxHands; ++j)
				{
					unsigned int tempId = m_hands[j]->m_id;
					if (tempId == id)
					{
						oldIndex = j;
						break;
					}
					if (firstFreeIndex == -1 && tempId == 0)
					{
						firstFreeIndex = j;
					}
				}
				if (oldIndex != -1)
				{
					// Found it, so swap him to here
					std::swap(m_hands[i], m_hands[oldIndex]);
					hand = m_hands[i];
				}
				else
				{
					// Not found so look what we can do with the one currently here...
					if (firstFreeIndex != -1)
					{
						// We have a free slot to which we can move the current hand
						std::swap(m_hands[i], m_hands[firstFreeIndex]);
					}
					else if (getHand(m_hands[i]->m_id) == m_hands[i])
					{
						// old hand still valid, but no other free slot available
						// so we have to drop him
						// Therefore, we remove the old map entry
						m_handIDToHands.erase(m_hands[i]->m_id);
					}
					// We now must have a usable hand slot at the current index
					hand = m_hands[i];
					// but we have to reset his old data
					hand->reset();
					// set correct id
					hand->m_id = id;
					// and set his map entry
					m_handIDToHands[id] = hand;
				}
			}

			// Now the hand has to be in the correct slot and everything should be set correctly
			
			bool wasTracked = hand->m_isTracked;
			// get the tracking data from the sensor
			hand->updateFingerTrackingData(m_fingerSensor);

			if (!wasTracked && hand->m_isTracked)
			{
				// Hand tracking has started for this one!
				// Autostart combination detection
				for (unsigned int j = 0; j < getNumUserDefinedCombinationRecognizers(); ++j)
				{
					CombinationRecognizer* rec = getUserDefinedCombinationRecognizer(j);
					hand->enableCombinationRecognition(rec, false);
					if (getAutoStartCombinationRecognition(Fubi::Combinations::NUM_COMBINATIONS))
						hand->enableCombinationRecognition(rec, true);
				}
			}
		}

		// invalidate all hands after the now corrected ones
		for (unsigned int i = m_numHands; i < MaxHands; ++i)
		{
			// invalid hand -> reset
			m_hands[i]->reset();
		}
	}
}

void FubiCore::updateUsers()
{
	static unsigned int userIDs[MaxUsers];

	if (m_sensor)
	{
		// Get current user ids
		m_numUsers = m_sensor->getUserIDs(userIDs);

		// First sort our user array according to the given id array
		for (unsigned int i = 0; i < m_numUsers; ++i)
		{
			unsigned int id = userIDs[i];
			FubiUser* user = m_users[i];
			if (user->m_id != id) // user at the wrong place or still unknown
			{
				// Try to find the user with the correct id (can only be later in the array as we already have corrected the ones before)
				// or at least find a free slot (at the current place or again later in the array)
				unsigned int oldIndex = -1;
				unsigned int firstFreeIndex = -1;
				for (unsigned int j = i; j < MaxUsers; ++j)
				{
					unsigned int tempId = m_users[j]->m_id;
					if (tempId == id)
					{
						oldIndex = j;
						break;
					}
					if (firstFreeIndex == -1 && tempId == 0)
					{
						firstFreeIndex = j;
					}
				}
				if (oldIndex != -1)
				{
					// Found him, so swap him to here
					std::swap(m_users[i], m_users[oldIndex]);
					user = m_users[i];
				}
				else
				{
					// Not found so look what we can do with the one currently here...
					if (firstFreeIndex != -1)
					{
						// We have a free slot to which we can move the current user
						std::swap(m_users[i], m_users[firstFreeIndex]);
					}
					else if (getUser(m_users[i]->m_id) == m_users[i])
					{
						// old user still valid, but no free slot available
						// so we have to drop him
						// Therefore, we remove the old map entry
						m_userIDToUsers.erase(m_users[i]->m_id);
					}
					// We now must have a usable user slot at the current index
					user = m_users[i];
					// but we have to reset his old data
					user->reset();
					// but keep him in scene
					user->m_inScene = true;
					// set correct id
					user->m_id = id;
					// and set his map entry
					m_userIDToUsers[id] = user;
				}
			}

			// Now the user has to be in the correct slot and everything should be set correctly
			
			bool wasTracked = user->m_isTracked;
			// get the tracking data from the sensor
			user->updateTrackingData(m_sensor);

			if (!wasTracked && user->m_isTracked)
			{
				// User tracking has started for this one!
				// Autostart posture combination detection
				for (unsigned int k = 0; k <Combinations::NUM_COMBINATIONS; ++k)
				{
					user->enableCombinationRecognition((Combinations::Combination)k,  false);
					if (getAutoStartCombinationRecognition((Combinations::Combination)k))
						user->enableCombinationRecognition((Combinations::Combination)k,  true);
				}
				// Special treatment for user definded posture combinations
				for (unsigned int j = 0; j < getNumUserDefinedCombinationRecognizers(); ++j)
				{
					CombinationRecognizer* rec = getUserDefinedCombinationRecognizer(j);
					user->enableCombinationRecognition(rec, false);
					if (getAutoStartCombinationRecognition(Fubi::Combinations::NUM_COMBINATIONS))
						user->enableCombinationRecognition(rec, true);
				}
			}
		}

		// invalidate all users after the now corrected ones
		for (unsigned int i = m_numUsers; i < MaxUsers; ++i)
		{
			// invalid user -> reset
			m_users[i]->reset();
		}
	}
}

unsigned int FubiCore::addJointRelationRecognizer(SkeletonJoint::Joint joint, SkeletonJoint::Joint relJoint,
	const Vec3f& minValues /*= Vec3f(-Math::MaxFloat,-Math::MaxFloat, -Math::MaxFloat)*/, 
	const Vec3f& maxValues /*= Vec3f(Math::MaxFloat, Math::MaxFloat, Math::MaxFloat)*/, 
	float minDistance /*= 0*/, 
	float maxDistance /*= Math::MaxFloat*/,
	bool useLocalPositions /*= false*/,
	int atIndex /*=  -1*/,
	const char* name /*= 0*/,
	float minConfidence /*=-1*/,
	Fubi::BodyMeasurement::Measurement measuringUnit /*= Fubi::BodyMeasurement::NUM_MEASUREMENTS*/,
		bool useFilteredData /*= false*/)
{
	string sName;
	if (name != 0)
		sName = name;
	// Add recognizer
	if (atIndex < 0 || (unsigned)atIndex >= m_userDefinedRecognizers.size())
	{
		// As a new one at the end
		atIndex = m_userDefinedRecognizers.size();
		m_userDefinedRecognizers.push_back(pair<string, IGestureRecognizer*>(sName, createPostureRecognizer(joint, relJoint, minValues, maxValues, minDistance, maxDistance, useLocalPositions, minConfidence, measuringUnit, useFilteredData)));
	}
	else
	{
		// Replacing an old one
		delete m_userDefinedRecognizers[atIndex].second;
		m_userDefinedRecognizers[atIndex].first = sName;
		m_userDefinedRecognizers[atIndex].second = createPostureRecognizer(joint, relJoint, minValues, maxValues, minDistance, maxDistance, useLocalPositions, minConfidence, measuringUnit, useFilteredData);
	}
	// Return index
	return atIndex;
}

unsigned int FubiCore::addJointOrientationRecognizer(SkeletonJoint::Joint joint,
		const Fubi::Vec3f& minValues /*= Fubi::Vec3f(-180.0f, -180.0f, -180.0f)*/, const Fubi::Vec3f& maxValues /*= Fubi::Vec3f(180.0f, 180.0f, 180.0f)*/,
		bool useLocalOrientations /*= true*/,
		int atIndex /*= -1*/,
		const char* name /*= 0*/,
		float minConfidence /*=-1*/,
		bool useFilteredData /*= false*/)
{
	string sName;
	if (name != 0)
		sName = name;
	// Add recognizer
	if (atIndex < 0 || (unsigned)atIndex >= m_userDefinedRecognizers.size())
	{
		// As a new one at the end
		atIndex = m_userDefinedRecognizers.size();
		m_userDefinedRecognizers.push_back(pair<string, IGestureRecognizer*>(sName, createPostureRecognizer(joint, minValues, maxValues, useLocalOrientations, minConfidence, useFilteredData)));
	}
	else
	{
		// Replacing an old one
		delete m_userDefinedRecognizers[atIndex].second;
		m_userDefinedRecognizers[atIndex].first = sName;
		m_userDefinedRecognizers[atIndex].second = createPostureRecognizer(joint, minValues, maxValues, useLocalOrientations, minConfidence, useFilteredData);
	}
	// Return index
	return atIndex;
}

unsigned int FubiCore::addJointOrientationRecognizer(Fubi::SkeletonJoint::Joint joint,
		const Fubi::Vec3f& orientation, float maxAngleDifference,
		bool useLocalOrientations /*= true*/,
		int atIndex /*= -1*/,
		const char* name /*= 0*/,
		float minConfidence /*=-1*/,
		bool useFilteredData /*= false*/)
{
	string sName;
	if (name != 0)
		sName = name;
	// Add recognizer
	if (atIndex < 0 || (unsigned)atIndex >= m_userDefinedRecognizers.size())
	{
		// As a new one at the end
		atIndex = m_userDefinedRecognizers.size();
		m_userDefinedRecognizers.push_back(pair<string, IGestureRecognizer*>(sName, createPostureRecognizer(joint, orientation, maxAngleDifference, useLocalOrientations, minConfidence, useFilteredData)));
	}
	else
	{
		// Replacing an old one
		delete m_userDefinedRecognizers[atIndex].second;
		m_userDefinedRecognizers[atIndex].first = sName;
		m_userDefinedRecognizers[atIndex].second = createPostureRecognizer(joint, orientation, maxAngleDifference, useLocalOrientations, minConfidence, useFilteredData);
	}
	// Return index
	return atIndex;
}

unsigned int FubiCore::addLinearMovementRecognizer(SkeletonJoint::Joint joint,	const Fubi::Vec3f& direction, float minVel, float maxVel /*= Fubi::Math::MaxFloat*/, 
		bool useLocalPositions /*= false*/,
		int atIndex /*= -1*/,
		const char* name /*= 0*/,
		float minConfidence /*=-1*/,
		float maxAngleDiff /*= 45.0f*/, 
		bool useOnlyCorrectDirectionComponent /*= true*/,
		bool useFilteredData /*= false*/)
{
	string sName;
	if (name != 0)
		sName = name;
	// Add recognizer
	if (atIndex < 0 || (unsigned)atIndex >= m_userDefinedRecognizers.size())
	{
		// As a new one at the end
		atIndex = m_userDefinedRecognizers.size();
		m_userDefinedRecognizers.push_back(pair<string, IGestureRecognizer*>(sName, createMovementRecognizer(joint, direction, minVel, maxVel, useLocalPositions, minConfidence, maxAngleDiff, useOnlyCorrectDirectionComponent, useFilteredData)));
	}
	else 
	{
		// Replacing an old one
		delete m_userDefinedRecognizers[atIndex].second;
		m_userDefinedRecognizers[atIndex].first = sName;
		m_userDefinedRecognizers[atIndex].second = createMovementRecognizer(joint, direction, minVel, maxVel, useLocalPositions, minConfidence, maxAngleDiff, useOnlyCorrectDirectionComponent, useFilteredData);
	}
	// Return index
	return atIndex;
}

unsigned int FubiCore::addLinearMovementRecognizer(SkeletonJoint::Joint joint, SkeletonJoint::Joint relJoint, 
	const Vec3f& direction, float minVel, float maxVel /*= Fubi::Math::MaxFloat*/,
	bool useLocalPositions /*= false*/,
	int atIndex /*=  -1*/, const char* name /*= 0*/,
	float minConfidence /*=-1*/,
	float maxAngleDiff /*= 45.0f*/,
	bool useOnlyCorrectDirectionComponent /*= true*/,
		bool useFilteredData /*= false*/)
{
	string sName;
	if (name != 0)
		sName = name;
	// Add recognizer
	if (atIndex < 0 || (unsigned)atIndex >= m_userDefinedRecognizers.size())
	{
		// As a new one at the end
		atIndex = m_userDefinedRecognizers.size();
		m_userDefinedRecognizers.push_back(pair<string, IGestureRecognizer*>(sName, createMovementRecognizer(joint, relJoint, direction, minVel, maxVel, useLocalPositions, minConfidence, maxAngleDiff, useOnlyCorrectDirectionComponent, useFilteredData)));
	}
	else 
	{
		// Replacing an old one
		delete m_userDefinedRecognizers[atIndex].second;
		m_userDefinedRecognizers[atIndex].first = sName;
		m_userDefinedRecognizers[atIndex].second = createMovementRecognizer(joint, relJoint, direction, minVel, maxVel, useLocalPositions, minConfidence, maxAngleDiff, useOnlyCorrectDirectionComponent, useFilteredData);
	}
	// Return index
	return atIndex;
}

unsigned int FubiCore::addLinearMovementRecognizer(SkeletonJoint::Joint joint,	const Fubi::Vec3f& direction, float minVel, float maxVel, 
	float minLength, float maxLength /*= Fubi::Math::MaxFloat*/,
	Fubi::BodyMeasurement::Measurement measuringUnit /*= Fubi::BodyMeasurement::NUM_MEASUREMENTS*/,
	bool useLocalPositions /*= false*/,
	int atIndex /*= -1*/,
	const char* name /*= 0*/,
	float minConfidence /*=-1*/,
	float maxAngleDiff /*= 45.0f*/, 
	bool useOnlyCorrectDirectionComponent /*= true*/,
	bool useFilteredData /*= false*/)
{
	string sName;
	if (name != 0)
		sName = name;
	// Add recognizer
	if (atIndex < 0 || (unsigned)atIndex >= m_userDefinedRecognizers.size())
	{
		// As a new one at the end
		atIndex = m_userDefinedRecognizers.size();
		m_userDefinedRecognizers.push_back(pair<string, IGestureRecognizer*>(sName, createMovementRecognizer(joint, direction, minVel, maxVel, minLength, maxLength, measuringUnit, useLocalPositions, minConfidence, maxAngleDiff, useOnlyCorrectDirectionComponent, useFilteredData)));
	}
	else 
	{
		// Replacing an old one
		delete m_userDefinedRecognizers[atIndex].second;
		m_userDefinedRecognizers[atIndex].first = sName;
		m_userDefinedRecognizers[atIndex].second = createMovementRecognizer(joint, direction, minVel, maxVel, minLength, maxLength, measuringUnit, useLocalPositions, minConfidence, maxAngleDiff, useOnlyCorrectDirectionComponent, useFilteredData);
	}
	// Return index
	return atIndex;
}

unsigned int FubiCore::addLinearMovementRecognizer(SkeletonJoint::Joint joint, SkeletonJoint::Joint relJoint, 
	const Vec3f& direction, float minVel, float maxVel,
	float minLength, float maxLength /*= Fubi::Math::MaxFloat*/,
	Fubi::BodyMeasurement::Measurement measuringUnit /*= Fubi::BodyMeasurement::NUM_MEASUREMENTS*/,
	bool useLocalPositions /*= false*/,
	int atIndex /*=  -1*/, const char* name /*= 0*/,
	float minConfidence /*=-1*/,
	float maxAngleDiff /*= 45.0f*/,
	bool useOnlyCorrectDirectionComponent /*= true*/,
		bool useFilteredData /*= false*/)
{
	string sName;
	if (name != 0)
		sName = name;
	// Add recognizer
	if (atIndex < 0 || (unsigned)atIndex >= m_userDefinedRecognizers.size())
	{
		// As a new one at the end
		atIndex = m_userDefinedRecognizers.size();
		m_userDefinedRecognizers.push_back(pair<string, IGestureRecognizer*>(sName, createMovementRecognizer(joint, relJoint, direction, minVel, maxVel, minLength, maxLength, measuringUnit, useLocalPositions, minConfidence, maxAngleDiff, useOnlyCorrectDirectionComponent, useFilteredData)));
	}
	else 
	{
		// Replacing an old one
		delete m_userDefinedRecognizers[atIndex].second;
		m_userDefinedRecognizers[atIndex].first = sName;
		m_userDefinedRecognizers[atIndex].second = createMovementRecognizer(joint, relJoint, direction, minVel, maxVel, minLength, maxLength, measuringUnit, useLocalPositions, minConfidence, maxAngleDiff, useOnlyCorrectDirectionComponent, useFilteredData);
	}
	// Return index
	return atIndex;
}

unsigned int FubiCore::addAngularMovementRecognizer(Fubi::SkeletonJoint::Joint joint, const Fubi::Vec3f& minAngularVelocity /*= Fubi::DefaultMinVec*/,
		const Fubi::Vec3f& maxAngularVelocity /*= Fubi::DefaultMaxVec*/,
		bool useLocalOrients /*= true*/,
		int atIndex /*= -1*/, const char* name /*= 0*/,
		float minConfidence /*= -1.0f*/, bool useFilteredData /*= false*/)
{
	string sName;
	if (name != 0)
		sName = name;
	// Add recognizer
	if (atIndex < 0 || (unsigned)atIndex >= m_userDefinedRecognizers.size())
	{
		// As a new one at the end
		atIndex = m_userDefinedRecognizers.size();
		m_userDefinedRecognizers.push_back(pair<string, IGestureRecognizer*>(sName, createMovementRecognizer(joint, minAngularVelocity, maxAngularVelocity, useLocalOrients, minConfidence, useFilteredData)));
	}
	else 
	{
		// Replacing an old one
		delete m_userDefinedRecognizers[atIndex].second;
		m_userDefinedRecognizers[atIndex].first = sName;
		m_userDefinedRecognizers[atIndex].second = createMovementRecognizer(joint, minAngularVelocity, maxAngularVelocity, useLocalOrients, minConfidence, useFilteredData);
	}
	// Return index
	return atIndex;
}

unsigned int FubiCore::addLinearAccelerationRecognizer(Fubi::SkeletonJoint::Joint joint,
		const Fubi::Vec3f& direction, float minAccel, float maxAccel /*= Fubi::Math::MaxFloat*/,
		int atIndex /*= -1*/,
		const char* name /*= 0*/,
		float minConfidence /*=-1*/,
		float maxAngleDifference /*= 45.0f*/, 
		bool useOnlyCorrectDirectionComponent /*= true*/)
{
	string sName;
	if (name != 0)
		sName = name;
	// Add recognizer
	if (atIndex < 0 || (unsigned)atIndex >= m_userDefinedRecognizers.size())
	{
		// As a new one at the end
		atIndex = m_userDefinedRecognizers.size();
		m_userDefinedRecognizers.push_back(pair<string, IGestureRecognizer*>(sName, createMovementRecognizer(joint, direction, minAccel, maxAccel, minConfidence, maxAngleDifference, useOnlyCorrectDirectionComponent)));
	}
	else 
	{
		// Replacing an old one
		delete m_userDefinedRecognizers[atIndex].second;
		m_userDefinedRecognizers[atIndex].first = sName;
		m_userDefinedRecognizers[atIndex].second = createMovementRecognizer(joint, direction, minAccel, maxAccel, minConfidence, maxAngleDifference, useOnlyCorrectDirectionComponent);
	}
	// Return index
	return atIndex;
}

unsigned int FubiCore::addFingerCountRecognizer(SkeletonJoint::Joint handJoint,
		unsigned int minFingers, unsigned int maxFingers,
		int atIndex /*= -1*/,
		const char* name /*= 0*/,
		float minConfidence /*=-1*/,
		bool useMedianCalculation /*= false*/,
		bool useFilteredData /*= false*/)
{
	string sName;
	if (name != 0)
		sName = name;
	// Add recognizer
	if (atIndex < 0 || (unsigned)atIndex >= m_userDefinedRecognizers.size())
	{
		// As a new one at the end
		atIndex = m_userDefinedRecognizers.size();
		m_userDefinedRecognizers.push_back(pair<string, IGestureRecognizer*>(sName, createPostureRecognizer(handJoint, minFingers, maxFingers, minConfidence, useMedianCalculation, useFilteredData)));
	}
	else
	{
		// Replacing an old one
		delete m_userDefinedRecognizers[atIndex].second;
		m_userDefinedRecognizers[atIndex].first = sName;
		m_userDefinedRecognizers[atIndex].second = createPostureRecognizer(handJoint, minFingers, maxFingers, minConfidence, useMedianCalculation, useFilteredData);
	}
	// Return index
	return atIndex;
}

unsigned short FubiCore::getCurrentUsers(FubiUser*** userContainer)
{
	if (userContainer != 0)
	{
		*userContainer = m_users;
	}
	return m_numUsers;
}

bool FubiCore::addCombinationRecognizer(const std::string& xmlDefinition)
{
	unsigned int oldNumCombs = m_userDefinedCombinationRecognizers.size();

	bool addedSomeThing = FubiXMLParser::loadCombinationRecognizer(xmlDefinition, m_userDefinedRecognizers, m_hiddenUserDefinedRecognizers, m_userDefinedCombinationRecognizers);

	if (addedSomeThing && getAutoStartCombinationRecognition(Fubi::Combinations::NUM_COMBINATIONS))
	{
		// Enable new combinations for all current users
		for (unsigned int user = 0; user < m_numUsers; user++)
		{
			for (unsigned int i = oldNumCombs; i < m_userDefinedCombinationRecognizers.size(); ++i)
			{
				m_users[user]->enableCombinationRecognition(m_userDefinedCombinationRecognizers[i].second, true);
			}
		}
	}

	return addedSomeThing;
}

bool FubiCore::loadRecognizersFromXML(const std::string& fileName)
{
	unsigned int oldNumCombs = m_userDefinedCombinationRecognizers.size();

	bool addedSomeThing =  FubiXMLParser::loadRecognizersFromXML(fileName, m_userDefinedRecognizers, m_hiddenUserDefinedRecognizers, m_userDefinedCombinationRecognizers);

	if (addedSomeThing && getAutoStartCombinationRecognition(Fubi::Combinations::NUM_COMBINATIONS))
	{
		// Enable new combinations for all current users
		for (unsigned int user = 0; user < m_numUsers; user++)
		{
			for (unsigned int i = oldNumCombs; i < m_userDefinedCombinationRecognizers.size(); ++i)
			{
				m_users[user]->enableCombinationRecognition(m_userDefinedCombinationRecognizers[i].second, true);
			}
		}
	}

	return addedSomeThing;
}

void FubiCore::getDepthResolution(int& width, int& height)
{
	if (m_sensor)
	{
		width = m_sensor->getDepthOptions().m_width;
		height = m_sensor->getDepthOptions().m_height;
	}
	else
	{
		width = -1;
		height = -1;
	}
}
void FubiCore::getRgbResolution(int& width, int& height)
{
	if (m_sensor)
	{
		width = m_sensor->getRgbOptions().m_width;
		height = m_sensor->getRgbOptions().m_height;
	}
	else
	{
		width = -1;
		height = -1;
	}
}
void FubiCore::getIRResolution(int& width, int& height)
{
	if (m_sensor)
	{
		width = m_sensor->getIROptions().m_width;
		height = m_sensor->getIROptions().m_height;
	}
	else
	{
		width = -1;
		height = -1;
	}
}

unsigned int FubiCore::getClosestUserID()
{
	FubiUser* user = getClosestUser();

	if (user)
	{
		return user->m_id;
	}

	return 0;
}

FubiUser* FubiCore::getClosestUser()
{
	std::deque<FubiUser*> closestUsers = getClosestUsers();

	if (!closestUsers.empty())
	{
		// Take the closest tracked user for posture rec
		return closestUsers.front();
	}
	return 0x0;
}

std::deque<unsigned int> FubiCore::getClosestUserIDs(int maxNumUsers /*= -1*/)
{
	std::deque<unsigned int> closestUserIDs;

	// Get closest users
	std::deque<FubiUser*> closestUsers = getClosestUsers(maxNumUsers);

	// Copy their ids
	std::deque<FubiUser*>::iterator iter;
	std::deque<FubiUser*>::iterator end = closestUsers.end();
	for (iter = closestUsers.begin(); iter != end; iter++)
	{
		// Take the closest tracked user for posture rec
		closestUserIDs.push_back((*iter)->m_id);
	}

	return closestUserIDs;
}

std::deque<FubiUser*> FubiCore::getClosestUsers(int maxNumUsers /*= -1*/)
{
	// Copy array into vector
	std::deque<FubiUser*> closestUsers;

	if (maxNumUsers != 0)
	{
		closestUsers.insert(closestUsers.begin(), m_users, m_users + m_numUsers);

		// Sort vector with special operator according to their distance in the x-z plane
		std::sort(closestUsers.begin(), closestUsers.end(), FubiUser::closerToSensor);

		if (maxNumUsers > 0)
		{
			// Now remove users with largest distance to meet the max user criteria
			while(closestUsers.size() > (unsigned)maxNumUsers)
				closestUsers.pop_back();
			// And sort the rest additionally from left to right
			std::sort(closestUsers.begin(), closestUsers.end(), FubiUser::moreLeft);
		}
	}

	return closestUsers;
}


void FubiCore::clearUserDefinedRecognizers()
{
	for (unsigned int i = 0; i < Fubi::MaxUsers; i++)
	{
		m_users[i]->clearUserDefinedCombinationRecognizers();
	}

	vector<pair<string, CombinationRecognizer*> >::iterator iter1;
	vector<pair<string, CombinationRecognizer*> >::iterator end1 = m_userDefinedCombinationRecognizers.end();
	for (iter1 = m_userDefinedCombinationRecognizers.begin(); iter1 != end1; ++iter1)
	{
		delete iter1->second;
	}
	m_userDefinedCombinationRecognizers.clear();

	vector<pair<string, IGestureRecognizer*> >::iterator iter;
	vector<pair<string, IGestureRecognizer*> >::iterator end = m_userDefinedRecognizers.end();
	for (iter = m_userDefinedRecognizers.begin(); iter != end; ++iter)
	{
		delete iter->second;
	}
	m_userDefinedRecognizers.clear();

	vector<pair<string, IGestureRecognizer*> >::iterator iter2;
	vector<pair<string, IGestureRecognizer*> >::iterator end2 = m_hiddenUserDefinedRecognizers.end();
	for (iter2 = m_hiddenUserDefinedRecognizers.begin(); iter2 != end2; ++iter2)
	{
		delete iter2->second;
	}
	m_hiddenUserDefinedRecognizers.clear();
}

void FubiCore::updateTrackingData(unsigned int userId, float* skeleton,	double timeStamp /*= -1*/, float* accelerations /*= 0*/)
{
	SkeletonJointPosition* skelPositions = 0;
	SkeletonJointOrientation* orientationMats = 0;
	SkeletonJointAcceleration* accels = 0;

	if (skeleton != 0)
	{
		skelPositions = new SkeletonJointPosition[SkeletonJoint::NUM_JOINTS];
		orientationMats = new SkeletonJointOrientation[SkeletonJoint::NUM_JOINTS];
		for (int i = 0; i < SkeletonJoint::NUM_JOINTS; i++)
		{
			int startIndex = i*8;
			skelPositions[i].m_position.x = skeleton[startIndex];
			skelPositions[i].m_position.y = skeleton[startIndex+1];
			skelPositions[i].m_position.z = skeleton[startIndex+2];
			skelPositions[i].m_confidence = skeleton[startIndex+3];

			float rotX = skeleton[startIndex+4];
			float rotY = skeleton[startIndex+5];
			float rotZ = skeleton[startIndex+6];
			orientationMats[i].m_orientation = Matrix3f::RotMat(degToRad(rotX), degToRad(rotY), degToRad(rotZ));
			orientationMats[i].m_confidence = skeleton[startIndex+7];
		}
	}

	if (accelerations != 0)
	{
		accels = new SkeletonJointAcceleration[SkeletonJoint::NUM_JOINTS];
		for (int i = 0; i < SkeletonJoint::NUM_JOINTS; i++)
		{
			int startIndex = i*4;
			accels[i].m_acceleration.x = accelerations[startIndex];
			accels[i].m_acceleration.y = accelerations[startIndex+1];
			accels[i].m_acceleration.z = accelerations[startIndex+2];
			accels[i].m_confidence = accelerations[startIndex+3];
		}

	}

	updateTrackingData(userId, skelPositions, timeStamp, orientationMats, accels);
	
	delete[] skelPositions;
	delete[] orientationMats;
	delete[] accels;
}

void FubiCore::updateTrackingData(unsigned int userId, Fubi::SkeletonJointPosition* positions,
		double timeStamp /*= -1*/, Fubi::SkeletonJointOrientation* orientations /*= 0*/, Fubi::SkeletonJointAcceleration* accelerations /*= 0*/)
{
	if (positions == 0 && orientations == 0 && accelerations == 0)
		return; // No data provided

	// First check if this is a new user
	map<unsigned int, FubiUser*>::iterator iter = m_userIDToUsers.find(userId);
	int index = -1;
	FubiUser* user = 0x0;
	if (iter == m_userIDToUsers.end())
	{
		// new User, new entry
		index = m_numUsers;
		user = m_userIDToUsers[userId] = m_users[index];
		user->m_id = userId;
		m_numUsers++;

		// Init the user info
		user->m_inScene = true;
		user->m_isTracked = true;

		// Autostart posture combination detection
		for (unsigned int i = 0; i <Combinations::NUM_COMBINATIONS; ++i)
		{
			user->enableCombinationRecognition((Combinations::Combination)i,  getAutoStartCombinationRecognition((Combinations::Combination)i));
		}
		// Special treatment for user defined posture combinations
		if (getAutoStartCombinationRecognition(Fubi::Combinations::NUM_COMBINATIONS))
		{
			for (unsigned int j = 0; j < getNumUserDefinedCombinationRecognizers(); ++j)
			{
				user->enableCombinationRecognition(getUserDefinedCombinationRecognizer(j), true);
			}
		}
	}
	else
		user = iter->second;

	// Now set the new tracking info for the user and let him do the rest of the updates
	if (positions || orientations)
		user->addNewTrackingData(positions, timeStamp, orientations);
	if (accelerations)
		user->addNewAccelerationData(accelerations, timeStamp);
	user->update();
}

Vec3f FubiCore::realWorldToProjective(const Vec3f& realWorldVec, int xRes /*= 640*/, int yRes /*= 480*/,
	double hFOV /*= 1.0144686707507438*/, double vFOV /*= 0.78980943449644714*/)
{
	Vec3f ret(Math::NO_INIT);

	if (m_sensor)
	{
		return m_sensor->realWorldToProjective(realWorldVec);
	}
	else
	{
		static const double realWorldXtoZ = tan(hFOV/2)*2;
		static const double realWorldYtoZ = tan(vFOV/2)*2;
		static const double coeffX = xRes / realWorldXtoZ;
		static const double coeffY = yRes / realWorldYtoZ;
		static const int nHalfXres = xRes / 2;
		static const int nHalfYres = yRes / 2;

		ret.x = (float)coeffX * realWorldVec.x / clamp(abs(realWorldVec.z), 1.0f, Math::MaxFloat) + nHalfXres;
		ret.y = nHalfYres - (float)coeffY * realWorldVec.y / clamp(abs(realWorldVec.z), 1.0f, Math::MaxFloat);
		ret.z = realWorldVec.z;
	}

	return ret;
}

void FubiCore::resetTracking()
{
	if (m_sensor)
	{
		for (unsigned short i = 0; i < m_numUsers; ++i)
		{
			m_sensor->resetTracking(m_users[i]->m_id);
		}
	}
}

bool FubiCore::initFingerSensor(FingerSensorType::Type type, const Fubi::Vec3f& offsetPos)
{
	delete m_fingerSensor;
	m_fingerSensor = 0x0;
	bool success = false;

	for (unsigned int i = 0; i < MaxHands; ++i)
	{
		m_hands[i]->m_id = 0;
		m_hands[i]->m_isTracked = false;
	}
	m_numHands = 0;
	m_handIDToHands.clear();

	if (type == FingerSensorType::LEAP)
	{
#ifdef FUBI_USE_LEAP
		m_fingerSensor = new FubiLeapSensor();
		success = m_fingerSensor->init(offsetPos);
#else
		Fubi_logErr("Leap sensor is not activated\n -Did you forget to uncomment the FUBI_USE_LEAP define in the FubiConfig.h?\n");	
#endif
	}
	else if (type == FingerSensorType::NONE)
	{
		Fubi_logInfo("FubiCore: Current finger sensor deactivated!\n");
		success = true;
	}

	if (!success)
	{
		delete m_fingerSensor;
		m_fingerSensor = 0x0;
	}

	return success;
}

Fubi::RecognizerTarget::Target FubiCore::getCombinationRecognizerTargetSensor(const char* recognizerName)
{
	CombinationRecognizer* rec = getUserDefinedCombinationRecognizer(recognizerName);
	if (rec)
	{
		return rec->getRecognizerTarget();
	}
	return RecognizerTarget::NO_SENSOR;
}

Fubi::RecognizerTarget::Target FubiCore::getRecognizerTargetSensor(const char* recognizerName)
{
	int recognizerIndex = getUserDefinedRecognizerIndex(recognizerName);
	if (recognizerIndex >= 0)
	{
		return m_userDefinedRecognizers[recognizerIndex].second->m_targetSensor;
	}
	return RecognizerTarget::NO_SENSOR;
}

const char* FubiCore::getCombinationRecognitionStateMetaInfo(const char* recognizerName, unsigned int stateIndex, const char* propertyName)
{
	if (propertyName && recognizerName)
	{
		CombinationRecognizer* rec = getUserDefinedCombinationRecognizer(recognizerName);
		if (rec)
		{
			const std::string& value = rec->getStateMetaInfo(stateIndex, propertyName);
			if (value.length() > 0)
				return value.c_str();
		}
	}
	return 0x0;
}
