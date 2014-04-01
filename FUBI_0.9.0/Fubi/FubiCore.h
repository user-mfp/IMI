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

#pragma once

// General includes
#include "Fubi.h"
#include "FubiUtils.h"
#include "FubiUser.h"
#include "FubiHand.h"
#include "FubiISensor.h"
#include "FubiIFingerSensor.h"

// Recognizer interfaces
#include "GestureRecognizer/IGestureRecognizer.h"
#include "GestureRecognizer/CombinationRecognizer.h"

// STL containers
#include <map>
#include <vector>
#include <string>
#include <set>

class FubiCore
{
public:
	// Singleton init only if not yet done
	static bool init(const char* xmlPath, Fubi::SkeletonTrackingProfile::Profile profile = Fubi::SkeletonTrackingProfile::ALL,
		float filterMinCutOffFrequency = 1.0f, float filterVelocityCutOffFrequency = 1.0f, float filterCutOffSlope = 0.007f,
		bool mirrorStream = true, bool registerStreams =true)
	{
		bool success = true;
		if (s_instance == 0x0)
		{
			s_instance = new FubiCore();
			if (xmlPath == 0x0)
			{
				Fubi_logInfo("FubiCore: Initialized in non-tracking mode!\n");
			}
			else
			{
				// init with xml file
				success = s_instance->initFromXml(xmlPath, profile, mirrorStream, registerStreams);
			}
			
			if (!success)
			{
				Fubi_logErr("Failed to inialize the sensor via XML!\n");
				delete s_instance;
				s_instance = 0x0;
			}
			else
			{
				Fubi_logInfo("FubiCore: Succesfully inialized the sensor via XML.\n");
				// Set filter options if succesful
				s_instance->setFilterOptions(filterMinCutOffFrequency, filterVelocityCutOffFrequency, filterCutOffSlope);
			}
		}
		else
		{
			Fubi_logWrn("Fubi already initalized. New init will be ignored!\n");
		}
		return success;
	}
	static bool init(const Fubi::SensorOptions& sensorOptions, const Fubi::FilterOptions& filterOptions)
	{
		bool success = true;
		if (s_instance == 0x0)
		{
			s_instance = new FubiCore();

			// init with options
			success = s_instance->initSensorWithOptions(sensorOptions);
			
			if (!success)
			{
				Fubi_logErr("Failed to inialize the sensor with the given options!\n");
				delete s_instance;
				s_instance = 0x0;
			}
			else
			{
				Fubi_logInfo("FubiCore: Succesfully inialized the sensor with the given options.\n");
				// Set filter options if succesful
				s_instance->setFilterOptions(filterOptions);
			}
		}
		else
		{
			Fubi_logWrn("Fubi already initalized. New init will be ignored!\n");
		}
		return success;
	}

	// Singleton getter (maybe null if not initialized!)
	static FubiCore* getInstance()
	{
		return s_instance;
	}

	// Release the singleton
	static void release()
	{
		delete s_instance;
		s_instance = 0x0;
	}

	// init an additional finger sensor
	bool initFingerSensor(Fubi::FingerSensorType::Type type, const Fubi::Vec3f& offsetPos);

	void updateSensor();

	// Get the floor plane
	Fubi::Plane getFloor();

	// Get current users as an array
	unsigned short getCurrentUsers(FubiUser*** userContainer);

	void getDepthResolution(int& width, int& height);
	void getRgbResolution(int& width, int& height);
	void getIRResolution(int& width, int& height);

	// Add user defined gestures/postures
	unsigned int addJointRelationRecognizer(Fubi::SkeletonJoint::Joint joint, Fubi::SkeletonJoint::Joint relJoint,
		const Fubi::Vec3f& minValues = Fubi::DefaultMinVec, 
		const Fubi::Vec3f& maxValues = Fubi::DefaultMaxVec, 
		float minDistance = 0, 
		float maxDistance = Fubi::Math::MaxFloat,
		bool useLocalPositions = false,
		int atIndex = -1,
		const char* name = 0,
		float minConfidence =-1,
		Fubi::BodyMeasurement::Measurement measuringUnit = Fubi::BodyMeasurement::NUM_MEASUREMENTS,
		bool useFilteredData = false);
	unsigned int addJointOrientationRecognizer(Fubi::SkeletonJoint::Joint joint,
		const Fubi::Vec3f& minValues = Fubi::Vec3f(-180.0f, -180.0f, -180.0f), const Fubi::Vec3f& maxValues = Fubi::Vec3f(180.0f, 180.0f, 180.0f),
		bool useLocalOrientations = true,
		int atIndex = -1,
		const char* name = 0,
		float minConfidence =-1,
		bool useFilteredData = false);
	unsigned int addJointOrientationRecognizer(Fubi::SkeletonJoint::Joint joint,
		const Fubi::Vec3f& orientation, float maxAngleDifference,
		bool useLocalOrientations = true,
		int atIndex = -1,
		const char* name = 0,
		float minConfidence =-1,
		bool useFilteredData = false);
	unsigned int addLinearMovementRecognizer(Fubi::SkeletonJoint::Joint joint, Fubi::SkeletonJoint::Joint relJoint, 
		const Fubi::Vec3f& direction, float minVel, float maxVel = Fubi::Math::MaxFloat, 
		bool useLocalPositions = false,
		int atIndex = -1,
		const char* name = 0,
		float minConfidence =-1,
		float maxAngleDiff = 45.0f, 
		bool useOnlyCorrectDirectionComponent = true,
		bool useFilteredData = false);
	unsigned int addLinearMovementRecognizer(Fubi::SkeletonJoint::Joint joint,	const Fubi::Vec3f& direction, float minVel, float maxVel = Fubi::Math::MaxFloat, 
		bool useLocalPositions = false,
		int atIndex = -1,
		const char* name = 0,
		float minConfidence =-1,
		float maxAngleDiff = 45.0f, 
		bool useOnlyCorrectDirectionComponent = true,
		bool useFilteredData = false);
	unsigned int addLinearMovementRecognizer(Fubi::SkeletonJoint::Joint joint, Fubi::SkeletonJoint::Joint relJoint, 
		const Fubi::Vec3f& direction, float minVel, float maxVel, 
		float minLength, float maxLength = Fubi::Math::MaxFloat,
		Fubi::BodyMeasurement::Measurement measuringUnit = Fubi::BodyMeasurement::NUM_MEASUREMENTS,
		bool useLocalPositions = false,
		int atIndex = -1,
		const char* name = 0,
		float minConfidence =-1,
		float maxAngleDiff = 45.0f, 
		bool useOnlyCorrectDirectionComponent = true,
		bool useFilteredData = false);
	unsigned int addLinearMovementRecognizer(Fubi::SkeletonJoint::Joint joint,	const Fubi::Vec3f& direction, float minVel, float maxVel, 
		float minLength, float maxLength = Fubi::Math::MaxFloat,
		Fubi::BodyMeasurement::Measurement measuringUnit = Fubi::BodyMeasurement::NUM_MEASUREMENTS,
		bool useLocalPositions = false,
		int atIndex = -1,
		const char* name = 0,
		float minConfidence =-1,
		float maxAngleDiff = 45.0f, 
		bool useOnlyCorrectDirectionComponent = true,
		bool useFilteredData = false);
	unsigned int addAngularMovementRecognizer(Fubi::SkeletonJoint::Joint joint, const Fubi::Vec3f& minAngularVelocity = Fubi::DefaultMinVec,
		const Fubi::Vec3f& maxAngularVelocity = Fubi::DefaultMaxVec,
		bool useLocalOrients = true, 
		int atIndex = -1,
		const char* name = 0,
		float minConfidence = -1.0f,
		bool useFilteredData = false);
	unsigned int addFingerCountRecognizer(Fubi::SkeletonJoint::Joint handJoint,
		unsigned int minFingers, unsigned int maxFingers,
		int atIndex = -1,
		const char* name = 0,
		float minConfidence =-1,
		bool useMedianCalculation = false,
		bool useFilteredData = false);
	unsigned int addLinearAccelerationRecognizer(Fubi::SkeletonJoint::Joint joint,
		const Fubi::Vec3f& direction, float minAccel, float maxAccel = Fubi::Math::MaxFloat,
		int atIndex = -1,
		const char* name = 0,
		float minConfidence =-1,
		float maxAngleDifference = 45.0f, 
		bool useOnlyCorrectDirectionComponent = true);

	// load a combination recognizer from a string that represents an xml node with the combination definition
	bool addCombinationRecognizer(const std::string& xmlDefinition);

	// Load recognizers out of an xml configuration file
	bool loadRecognizersFromXML(const std::string& fileName);

	// Stop and remove all user defined recognizers
	void clearUserDefinedRecognizers();

	// Check current progress in gesture/posture recognition
	Fubi::RecognitionResult::Result recognizeGestureOn(Fubi::Postures::Posture postureID, unsigned int userID);
	Fubi::RecognitionResult::Result recognizeGestureOn(unsigned int recognizerIndex, unsigned int userID, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);
	Fubi::RecognitionResult::Result recognizeGestureOn(const std::string& recognizerName, unsigned int userID, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);
	Fubi::RecognitionResult::Result recognizeGestureOnHand(const std::string& recognizerName, unsigned int handID);


	// Enable a posture combination recognition manually
	void enableCombinationRecognition(Fubi::Combinations::Combination combinationID, unsigned int userID, bool enable);
	// Enable a user defined posture combination recognition manually
	void enableCombinationRecognition(const std::string& combinationName, unsigned int userID, bool enable);
	void enableCombinationRecognitionHand(const std::string& combinationName, unsigned int handID, bool enable);
	// Or auto activate all for each new user
	void setAutoStartCombinationRecognition(bool enable, Fubi::Combinations::Combination combinationID = Fubi::Combinations::NUM_COMBINATIONS);
	bool getAutoStartCombinationRecognition(Fubi::Combinations::Combination combinationID = Fubi::Combinations::NUM_COMBINATIONS);

	// Get number of user defined recognizers
	unsigned int getNumUserDefinedRecognizers() { return m_userDefinedRecognizers.size(); }
	// Get given name of a recognizer or an empty string in case of failure
	const std::string& getUserDefinedRecognizerName(unsigned int index) { return (index < m_userDefinedRecognizers.size()) ? m_userDefinedRecognizers[index].first : s_emtpyString; }
	// Get index of a recognizer with the given name or -1 in case of failure
	int getUserDefinedRecognizerIndex(const std::string& name);
	// Get index of a recognizer with the given name or -1 in case of failure
	int getHiddenUserDefinedRecognizerIndex(const std::string& name);

	// Get number of user defined recognizers
	unsigned int getNumUserDefinedCombinationRecognizers() { return m_userDefinedCombinationRecognizers.size(); }
	// Get given name of a recognizer or an empty string in case of failure
	const std::string& getUserDefinedCombinationRecognizerName(unsigned int index) { return (index < m_userDefinedCombinationRecognizers.size()) ? m_userDefinedCombinationRecognizers[index].first : s_emtpyString; }
	// Get index of a recognizer with the given name or -1 in case of failure
	int getUserDefinedCombinationRecognizerIndex(const std::string& name);
	CombinationRecognizer* getUserDefinedCombinationRecognizer(const std::string& name);
	CombinationRecognizer* getUserDefinedCombinationRecognizer(unsigned int index);
	// Get meta info of a combination recognizer state
	const char* getCombinationRecognitionStateMetaInfo(const char* recognizerName, unsigned int stateIndex, const char* propertyName);

	/**
	 * \brief Get the target sensor of a user defined combination recognizer
	 * 
	 * @param recognizerName name of the combination
	 * @return the target sensor as defined in FubiUtils.h
	 */
	Fubi::RecognizerTarget::Target getCombinationRecognizerTargetSensor(const char* recognizerName);

	/**
	 * \brief Get the target sensor for a recognizer
	 * 
	 * @param recognizerName name of the recognizer
	 * @return the target sensor as defined in FubiUtils.h
	 */
	Fubi::RecognizerTarget::Target getRecognizerTargetSensor(const char* recognizerName);

	// Get the id (starting with 1) of a user by its index (starting with 0). Returns 0 if not found
	unsigned int getUserID(unsigned int index)
	{
		if (index < m_numUsers)
			return m_users[index]->m_id;
		return 0;
	}
	unsigned int getNumUsers() { return m_numUsers; }

	// Get user by id
	FubiUser* getUser(unsigned int userId);

	// Get the user standing closest to the sensor
	FubiUser* getClosestUser();
	unsigned int getClosestUserID();
	std::deque<unsigned int> getClosestUserIDs(int maxNumUsers = -1);
	std::deque<FubiUser*> getClosestUsers(int maxNumUsers = -1);

	unsigned int getHandID(unsigned int index)
	{
		if (index < m_numHands)
			return m_hands[index]->m_id;
		return 0;
	}
	unsigned int getNumHands() { return m_numHands; } 
	FubiHand* getHand(unsigned int handID);
	FubiIFingerSensor* getFingerSensor() { return m_fingerSensor; }


	/**
	 * \brief Set the current tracking info of one user
	 * (including all joint positions and the center of mass. Optionally the orientations and a timestamp)
	 *
	 * @param userID OpenNI id of the user
	 * @param positions an array of the joint positions
	 * @param timestamp the timestamp of the tracking value (if -1 an own timestamp will be created)
	 * @param orientations an array of the joint positions (if 0, the orientations will be approximated from the given positions)
	 * @param acceleartions an array of joint accelerations (optional)
	 */
	void updateTrackingData(unsigned int userId, Fubi::SkeletonJointPosition* positions, 
		double timeStamp = -1, Fubi::SkeletonJointOrientation* orientations = 0, Fubi::SkeletonJointAcceleration* accelerations = 0);
	/* same function as before, but skeleton and acceleartion as a plain float array ,
		@param skeleton i.e. NUM_JOINTS * (position+orientation) with position, orientation all as 4 floats (x,y,z,conf) in milimeters or degrees
		@param acceleartions i.e. NUM_JOINTS * (acceleartion) with accelerations ass 4 floats (x,y,z,conf) in milimeters / second²
	   timeStamp in seconds or -1 for self calculation*/
	void updateTrackingData(unsigned int userId, float* skeleton, double timeStamp = -1, float* accelerations = 0);

	// Return realworld to projective according to current sensor, or approximate it according to standard Kinect/Xtion values (if no sensor present)
	Fubi::Vec3f realWorldToProjective(const Fubi::Vec3f& realWorldVec, int xRes = 640, int yRes = 480,
		double hFOV = 1.0144686707507438, double vFOV = 0.78980943449644714);

	// Reset the tracking of all users in the current sensor
	void resetTracking();

	FubiISensor* getSensor() { return m_sensor; }

	// initialize sensor with an options file
	bool initSensorWithOptions(const Fubi::SensorOptions& options);

	// getter/setter for filter options
	void setFilterOptions(const Fubi::FilterOptions& options)
	{
		m_filterOptions = options;
	}
	void setFilterOptions(float minCutOffFrequency = 1.0f, float velocityCutOffFrequency = 1.0f, float cutOffSlope = 0.007f)
	{
		m_filterOptions.m_minCutOffFrequency = minCutOffFrequency;
		m_filterOptions.m_velocityCutOffFrequency = velocityCutOffFrequency;
		m_filterOptions.m_cutOffSlope = cutOffSlope;
	}
	void getFilterOptions(float& filterMinCutOffFrequency, float& filterVelocityCutOffFrequency, float& filterCutOffSlope)
	{
		filterMinCutOffFrequency = m_filterOptions.m_minCutOffFrequency;
		filterVelocityCutOffFrequency = m_filterOptions.m_velocityCutOffFrequency;
		filterCutOffSlope = m_filterOptions.m_cutOffSlope;
	}

	// get/set the offset position of the finger sensor in relation to the main sensor
	const Fubi::Vec3f& getFingerSensorOffsetPosition() const { return m_fingerSensor ? m_fingerSensor->getOffsetPosition() : Fubi::NullVec; }
	void setFingerSensorOffsetPosition(const Fubi::Vec3f& offsetPos) { if (m_fingerSensor) m_fingerSensor->setOffsetPosition(offsetPos); }

private:
	// private constructor/destructor as it is a singeleton
	FubiCore();
	~FubiCore();

	// initialize with an xml file
	bool initFromXml(const char* xmlPath, Fubi::SkeletonTrackingProfile::Profile profile = Fubi::SkeletonTrackingProfile::ALL,
		bool mirrorStream = true, bool registerStreams =true);

	// Update FubiUser -> OpenNI/KinectSDK ID mapping and tracking data
	void updateUsers();

	// Update hand ids and tracking data
	void updateHands();

	// The singleton instance of the tracker
	static FubiCore* s_instance;

	const static std::string s_emtpyString;

	// Number of current users
	unsigned short m_numUsers;
	// All users
	FubiUser* m_users[Fubi::MaxUsers];
	// Mapping of user ids to users
	std::map<unsigned int, FubiUser*> m_userIDToUsers;

	// Number of current hands
	unsigned short m_numHands;
	// All Hands
	FubiHand* m_hands[Fubi::MaxHands];
	// Mapping of user ids to users
	std::map<unsigned int, FubiHand*> m_handIDToHands;

	// One posture recognizer per posture
	IGestureRecognizer* m_postureRecognizers[Fubi::Postures::NUM_POSTURES];

	// User defined recognizers (joint relations and linear gestures) stored with name
	std::vector<std::pair<std::string, IGestureRecognizer*> > m_userDefinedRecognizers;
	// Hidden user defined recognizers (joint relations and linear gestures) stored with name,
	// can only be used in Combinations, but not directly
	std::vector<std::pair<std::string, IGestureRecognizer*> > m_hiddenUserDefinedRecognizers;
	// User defined Combination recognizers (templates to apply for each user)
	std::vector<std::pair<std::string, CombinationRecognizer*> > m_userDefinedCombinationRecognizers;

	// The Combination recognizers that should start automatically when a new user is detected
	bool m_autoStartCombinationRecognizers[Fubi::Combinations::NUM_COMBINATIONS+1];

	// The sensor for getting stream and tracking data
	FubiISensor* m_sensor;

	// Additional sensor for getting finger tracking data
	FubiIFingerSensor* m_fingerSensor;

	// The filter options
	Fubi::FilterOptions m_filterOptions;
};