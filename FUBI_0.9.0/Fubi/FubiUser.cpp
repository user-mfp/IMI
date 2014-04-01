// ****************************************************************************************
//
// Fubi User
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************

#include "FubiUser.h"

#include "FubiISensor.h"
#include "FubiUtils.h"
#include "FubiCore.h"
#include "FubiImageProcessing.h"
#include "FubiRecognizerFactory.h"
#include "GestureRecognizer/CombinationRecognizer.h"

using namespace Fubi;

FubiUser::FubiUser() : m_inScene(false), m_id(0), m_isTracked(false),
	m_lastRightFingerDetection(-1), m_lastLeftFingerDetection(-1), m_fingerTrackIntervall(0.1),
	m_maxFingerCountForMedian(10), m_useConvexityDefectMethod(false),
	m_lastBodyMeasurementUpdate(0)
{
	//  Init tracking data
	m_currentTrackingData = new TrackingData();
	m_lastTrackingData = new TrackingData();
	m_filteredTrackingData = new TrackingData();
	m_lastFilteredTrackingData = new TrackingData();

	m_accelerationData = new AccelerationData();

	// And additional init for filtered data
	memset(m_lastFilteredVelocity, 0, sizeof(Fubi::Vec3f)*Fubi::SkeletonJoint::NUM_JOINTS);


	// Init the combination recognizers
	for (unsigned int i = 0; i < Combinations::NUM_COMBINATIONS; ++i)
	{
		m_combinationRecognizers[i] = Fubi::createCombinationRecognizer(this, (Combinations::Combination) i);
	}
}


FubiUser::~FubiUser()
{
	for (unsigned int i = 0; i < Combinations::NUM_COMBINATIONS; ++i)
	{
		delete m_combinationRecognizers[i];
		m_combinationRecognizers[i] = 0x0;
	}

	clearUserDefinedCombinationRecognizers();

	// Release any left over image data
	FubiImageProcessing::releaseImage(m_leftFingerCountImage.image);
	FubiImageProcessing::releaseImage(m_rightFingerCountImage.image);

	delete m_currentTrackingData;
	delete m_lastTrackingData;
	delete m_filteredTrackingData;
	delete m_lastFilteredTrackingData;

	delete m_accelerationData;
}

void FubiUser::clearUserDefinedCombinationRecognizers()
{
	std::map<std::string, CombinationRecognizer*>::iterator iter;
	std::map<std::string, CombinationRecognizer*>::iterator end = m_userDefinedCombinationRecognizers.end();
	for (iter = m_userDefinedCombinationRecognizers.begin(); iter != end; ++iter)
	{
		delete iter->second;
	}
	m_userDefinedCombinationRecognizers.clear();
}



void FubiUser::enableCombinationRecognition(Fubi::Combinations::Combination postureID, bool enable)
{
	if (postureID < Fubi::Combinations::NUM_COMBINATIONS && m_combinationRecognizers[postureID])
	{
		if (enable)
			m_combinationRecognizers[postureID]->start();
		else
			m_combinationRecognizers[postureID]->stop();
	}
}

void FubiUser::enableCombinationRecognition(const CombinationRecognizer* recognizerTemplate, bool enable)
{
	if (recognizerTemplate)
	{
		std::map<std::string, CombinationRecognizer*>::iterator rec = m_userDefinedCombinationRecognizers.find(recognizerTemplate->getName());
		if (rec != m_userDefinedCombinationRecognizers.end())
		{
			if (enable)
				rec->second->start();
			else
			{
				rec->second->stop();
			}
		}
		else if (enable  && recognizerTemplate->getRecognizerTarget() == RecognizerTarget::ALL_SENSORS || recognizerTemplate->getRecognizerTarget() == RecognizerTarget::BODY_SENSOR)
		{
			CombinationRecognizer* clonedRec = recognizerTemplate->clone();
			clonedRec->setUser(this);
			m_userDefinedCombinationRecognizers[recognizerTemplate->getName()] = clonedRec;
			clonedRec->start();
		}
	}
}

void FubiUser::enableFingerTracking(bool leftHand, bool rightHand, bool useConvexityDefectMethod /*= false*/)
{
	bool enabledAnything = false;
	if (leftHand)
	{
		if (m_lastLeftFingerDetection == -1)
		{
			m_leftFingerCount.clear();
			m_lastLeftFingerDetection = 0;
			enabledAnything = true;
		}
	}
	else
		m_lastLeftFingerDetection = -1;

	if (rightHand)
	{
		if(m_lastRightFingerDetection == -1)
		{
			m_rightFingerCount.clear();
			m_lastRightFingerDetection = 0;
			enabledAnything = true;
		}
	}
	else
		m_lastRightFingerDetection = -1;

	if (enabledAnything)
		// Immediatley update the finger count
		updateFingerCount();
}

void FubiUser::addFingerCount(int count, bool leftHand /*= false*/)
{
	if (leftHand)
	{
		if (m_lastLeftFingerDetection > -1)
		{
			if (count > -1)
			{
				m_leftFingerCount.push_back(count);
				if (m_leftFingerCount.size() > m_maxFingerCountForMedian)
					m_leftFingerCount.pop_front();
			}
			m_lastLeftFingerDetection = Fubi::currentTime();
		}
	}
	else
	{
		if (m_lastRightFingerDetection > -1)
		{
			if (count > -1)
			{
				m_rightFingerCount.push_back(count);
				if (m_rightFingerCount.size() > m_maxFingerCountForMedian)
					m_rightFingerCount.pop_front();
			}
			m_lastRightFingerDetection = Fubi::currentTime();
		}
	}
}

int FubiUser::calculateMedianFingerCount(const std::deque<int>& fingerCount)
{
	int median = -1;
	std::priority_queue<int> sortedQueue;
	
	// Sort values in a queue
	std::deque<int>::const_iterator it;
	std::deque<int>::const_iterator end = fingerCount.end();
	for (it = fingerCount.begin(); it != end; ++it)
	{
		sortedQueue.push(*it);
	}

	// Throw away first half of the sorted queue
	int half = sortedQueue.size() / 2;
	for (int i = 0; i < half; ++i)
	{
		sortedQueue.pop();		
	}

	// Median is now on top
	if (!sortedQueue.empty())
	{
		median = sortedQueue.top();
	}

	return median;
}

int FubiUser::getFingerCount(bool leftHand /*= false*/, bool getMedianOfLastFrames /*= true*/, bool useOldConvexityDefectMethod /*= false*/)
{
	int fingerCount = -1;

	if (getMedianOfLastFrames)
	{
		fingerCount = calculateMedianFingerCount( leftHand ? m_leftFingerCount : m_rightFingerCount);		
	}

	if (fingerCount == -1 && FubiCore::getInstance())
	{
		// No precalculations present or wanted, so calculate one instantly

		// Prepare image debug data
		FingerCountImageData* debugData = leftHand ? (&m_leftFingerCountImage) : (&m_rightFingerCountImage);
		FubiImageProcessing::releaseImage(debugData->image);
		debugData->image = 0x0;

		// Now get the finger count
		fingerCount = FubiImageProcessing::applyFingerCount(FubiCore::getInstance()->getSensor(), m_id, leftHand, 
							useOldConvexityDefectMethod, debugData);
	}

	return fingerCount;
}

void FubiUser::updateTrackingData(FubiISensor* sensor)
{
	if (sensor)
	{
		// First update tracking state
		m_isTracked = sensor->isTracking(m_id);

		if (sensor->hasNewTrackingData())
		{
			//printf("FrameTime: %.6f\n", Fubi::currentTime()-m_currentTrackingData->timeStamp);

			// Swap tracking data pointers to leave a backup of the current one
			std::swap(m_lastTrackingData, m_currentTrackingData);
			std::swap(m_lastFilteredTrackingData, m_filteredTrackingData);			

			// Update timestamp
			m_currentTrackingData->timeStamp = m_filteredTrackingData->timeStamp = Fubi::currentTime();
			// The other joints are only valid if the user is tracked
			if (m_isTracked)
			{
				// Get all joint positions for that user
				for (unsigned int j=0; j < SkeletonJoint::NUM_JOINTS; ++j)
				{
					sensor->getSkeletonJointData(m_id, (SkeletonJoint::Joint) j, m_currentTrackingData->jointPositions[j], m_currentTrackingData->jointOrientations[j]);
				}

				update();
			}
			else
			{
				// Only try to get the torso (should be independent of complete tracking)
				sensor->getSkeletonJointData(m_id, SkeletonJoint::TORSO, m_currentTrackingData->jointPositions[SkeletonJoint::TORSO], m_currentTrackingData->jointOrientations[SkeletonJoint::TORSO]);
			}
		}
	}
}


void FubiUser::addNewTrackingData(Fubi::SkeletonJointPosition* positions,
	double timeStamp /*= -1*/, Fubi::SkeletonJointOrientation* orientations /*= 0*/)
{
	// Swap tracking data pointers to leave a backup of the current one
	std::swap(m_lastTrackingData, m_currentTrackingData);
	std::swap(m_lastFilteredTrackingData, m_filteredTrackingData);

	// Update timestamp
	if (timeStamp >= 0)
		m_currentTrackingData->timeStamp = m_filteredTrackingData->timeStamp = timeStamp;
	else
		m_currentTrackingData->timeStamp = m_filteredTrackingData->timeStamp = Fubi::currentTime();

	// Set new transformations
	for (unsigned int j=0; j < SkeletonJoint::NUM_JOINTS; ++j)
	{
		SkeletonJoint::Joint joint = (SkeletonJoint::Joint) j;
		// Copy new tracking data
		m_currentTrackingData->jointPositions[joint] = positions[j];
		if (orientations)
			m_currentTrackingData->jointOrientations[joint] = orientations[j];
	}

	if (orientations == 0)
	{
		// Try to calculate global orientations from position data (currently not very accurate)
		approximateGlobalOrientations(m_currentTrackingData->jointPositions, m_currentTrackingData->jointOrientations);
	}
}

void FubiUser::addNewAccelerationData(Fubi::SkeletonJointAcceleration* acceleration, double timeStamp /*= 0*/)
{
	// Update timestamp
	if (timeStamp >= 0)
		m_accelerationData->timeStamp = timeStamp;
	else
		m_accelerationData->timeStamp = Fubi::currentTime();

	// Set new transformations
	for (unsigned int j=0; j < SkeletonJoint::NUM_JOINTS; ++j)
	{
		SkeletonJoint::Joint joint = (SkeletonJoint::Joint) j;
		// Copy new tracking data
		m_accelerationData->accelerations[joint] = acceleration[j];
	}
}


void FubiUser::updateCombinationRecognizers()
{
	// Update the posture combination recognizers
	for (unsigned int i=0; i < Fubi::Combinations::NUM_COMBINATIONS; ++i)
	{
		if (m_combinationRecognizers[i])
		{
			if (!m_combinationRecognizers[i]->isActive() && Fubi::getAutoStartCombinationRecognition((Fubi::Combinations::Combination)i))
			{
				// Reactivate combination recognizers that should already be active
				m_combinationRecognizers[i]->start();
			}
			m_combinationRecognizers[i]->update();
		}
	}
	std::map<std::string, CombinationRecognizer*>::iterator iter;
	std::map<std::string, CombinationRecognizer*>::iterator end = m_userDefinedCombinationRecognizers.end();
	for (iter = m_userDefinedCombinationRecognizers.begin(); iter != end; ++iter)
	{
		if (iter->second)
		{
			if (!iter->second->isActive() && Fubi::getAutoStartCombinationRecognition())
			{
				// Reactivate combination recognizers that should already be active
				iter->second->start();
			}
			iter->second->update();
		}
	}
}

void FubiUser::calculateFilteredTransformations()
{
	// Apply filter to current global positions and orientations
	FubiCore* core = FubiCore::getInstance();
	if (core && m_currentTrackingData->timeStamp > m_lastTrackingData->timeStamp)
	{
		// Get filter config from the core
		float cutOffSlope, minCutOff, velocityCutOffFrequency;
		core->getFilterOptions(minCutOff, velocityCutOffFrequency, cutOffSlope);

		// Passed time since last update
		const float timeStep = float(m_currentTrackingData->timeStamp - m_lastTrackingData->timeStamp);

		// Global alpha values for the velocity
		const float velAlpha = oneEuroAlpha(timeStep, velocityCutOffFrequency);
		const float invVelAlpha = 1.0f-velAlpha;

		for (unsigned int i=0; i < SkeletonJoint::NUM_JOINTS; ++i)
		{
			// Calculate velocity as position difference..
			Vec3f vel = m_currentTrackingData->jointPositions[i].m_position - m_lastFilteredTrackingData->jointPositions[i].m_position;
			// ..divided by time difference
			vel /= timeStep;
			// Now low-pass filter the velocity
			vel = vel*velAlpha + m_lastFilteredVelocity[i]*invVelAlpha;
			// And save it
			m_lastFilteredVelocity[i] = vel;
			// Calculate the alpha values for the position filtering out of the filtered velocity
			Vec3f posAlpha(Math::NO_INIT);
			posAlpha.x = oneEuroAlpha(timeStep, minCutOff + cutOffSlope * abs(vel.x));
			posAlpha.y = oneEuroAlpha(timeStep, minCutOff + cutOffSlope * abs(vel.y));
			posAlpha.z = oneEuroAlpha(timeStep, minCutOff + cutOffSlope * abs(vel.z));
			Vec3f invPosAlpha(1.0f-posAlpha.x, 1.0f-posAlpha.y, 1.0f-posAlpha.z);
			// Now we can finally apply the filter
			m_filteredTrackingData->jointPositions[i].m_position = (m_currentTrackingData->jointPositions[i].m_position*posAlpha) + (m_lastFilteredTrackingData->jointPositions[i].m_position*invPosAlpha);
			// Calculate confidence according to the average alpha value
			float avgAlpha = (posAlpha.x+posAlpha.y+posAlpha.z)/3.0f;
			m_filteredTrackingData->jointPositions[i].m_confidence = avgAlpha*m_currentTrackingData->jointPositions[i].m_confidence + (1.0f-avgAlpha)*m_lastFilteredTrackingData->jointPositions[i].m_confidence;

			// For the orientations, we directly use the min cut off frequency without any velocity dependency
			m_filteredTrackingData->jointOrientations[i].m_orientation = (m_currentTrackingData->jointOrientations[i].m_orientation*minCutOff) + (m_lastFilteredTrackingData->jointOrientations[i].m_orientation*(1.0f-minCutOff));
			// Calculate confidence according to the alpha value
			m_filteredTrackingData->jointOrientations[i].m_confidence = minCutOff*m_currentTrackingData->jointOrientations[i].m_confidence + (1.0f-minCutOff)*m_lastFilteredTrackingData->jointOrientations[i].m_confidence;
		}
	}
}

void FubiUser::updateFingerCount()
{
	// Check and update finger detection
	if (m_lastLeftFingerDetection > -1
		&& (Fubi::currentTime() - m_lastLeftFingerDetection) > m_fingerTrackIntervall)
	{
		addFingerCount(getFingerCount(true, false, m_useConvexityDefectMethod), true);
	}
	if (m_lastRightFingerDetection > -1
		&& (Fubi::currentTime() - m_lastRightFingerDetection) > m_fingerTrackIntervall)
	{
		addFingerCount(getFingerCount(false, false, m_useConvexityDefectMethod), false);
	}
}

void FubiUser::updateBodyMeasurements()
{
	static const float filterFac = 0.1f;
	static const float updateIntervall = 0.5f;

	// Only once per second
	if (Fubi::currentTime()-m_lastBodyMeasurementUpdate > updateIntervall)
	{
		m_lastBodyMeasurementUpdate = Fubi::currentTime();

		// Select joints
		SkeletonJoint::Joint footToTake = SkeletonJoint::RIGHT_FOOT;
		SkeletonJoint::Joint kneeForFoot = SkeletonJoint::RIGHT_KNEE;
		if (m_currentTrackingData->jointPositions[SkeletonJoint::LEFT_FOOT].m_confidence > m_currentTrackingData->jointPositions[SkeletonJoint::RIGHT_FOOT].m_confidence)
		{
			footToTake = SkeletonJoint::LEFT_FOOT;
			kneeForFoot = SkeletonJoint::LEFT_KNEE;
		}
		SkeletonJoint::Joint hipToTake = SkeletonJoint::RIGHT_HIP;
		SkeletonJoint::Joint kneeForHip = SkeletonJoint::RIGHT_KNEE;
		if (m_currentTrackingData->jointPositions[SkeletonJoint::LEFT_KNEE].m_confidence > m_currentTrackingData->jointPositions[SkeletonJoint::RIGHT_KNEE].m_confidence)
		{
			hipToTake = SkeletonJoint::LEFT_HIP;
			kneeForHip = SkeletonJoint::LEFT_KNEE;
		}
		SkeletonJoint::Joint handToTake = SkeletonJoint::RIGHT_HAND;
		SkeletonJoint::Joint elbowForHand = SkeletonJoint::RIGHT_ELBOW;
		if (m_currentTrackingData->jointPositions[SkeletonJoint::LEFT_HAND].m_confidence > m_currentTrackingData->jointPositions[SkeletonJoint::RIGHT_HAND].m_confidence)
		{
			handToTake = SkeletonJoint::LEFT_HAND;
			elbowForHand = SkeletonJoint::LEFT_ELBOW;
		}
		SkeletonJoint::Joint shoulderToTake = SkeletonJoint::RIGHT_SHOULDER;
		SkeletonJoint::Joint elbowForShoulder = SkeletonJoint::RIGHT_ELBOW;
		if (m_currentTrackingData->jointPositions[SkeletonJoint::LEFT_ELBOW].m_confidence > m_currentTrackingData->jointPositions[SkeletonJoint::RIGHT_ELBOW].m_confidence)
		{
			shoulderToTake = SkeletonJoint::LEFT_SHOULDER;
			elbowForShoulder = SkeletonJoint::LEFT_ELBOW;
		}

		// Body height
		//Add the neck-head distance to compensate for the missing upper head part
		SkeletonJointPosition headEnd(m_currentTrackingData->jointPositions[SkeletonJoint::HEAD]);
		headEnd.m_position = headEnd.m_position + (headEnd.m_position-m_currentTrackingData->jointPositions[SkeletonJoint::NECK].m_position);
		Fubi::calculateBodyMeasurement(m_currentTrackingData->jointPositions[footToTake],
			headEnd, m_bodyMeasurements[BodyMeasurement::BODY_HEIGHT], filterFac);

		// Torso height
		Fubi::calculateBodyMeasurement(m_currentTrackingData->jointPositions[SkeletonJoint::WAIST],
			m_currentTrackingData->jointPositions[SkeletonJoint::NECK], m_bodyMeasurements[BodyMeasurement::TORSO_HEIGHT],filterFac);

		// Shoulder width
		Fubi::calculateBodyMeasurement(m_currentTrackingData->jointPositions[SkeletonJoint::LEFT_SHOULDER],
			m_currentTrackingData->jointPositions[SkeletonJoint::RIGHT_SHOULDER], m_bodyMeasurements[BodyMeasurement::SHOULDER_WIDTH],filterFac);

		// Hip width
		Fubi::calculateBodyMeasurement(m_currentTrackingData->jointPositions[SkeletonJoint::LEFT_HIP],
			m_currentTrackingData->jointPositions[SkeletonJoint::RIGHT_HIP], m_bodyMeasurements[BodyMeasurement::HIP_WIDTH],filterFac);

		// Arm lengths
		Fubi::calculateBodyMeasurement(m_currentTrackingData->jointPositions[shoulderToTake],
			m_currentTrackingData->jointPositions[elbowForShoulder], m_bodyMeasurements[BodyMeasurement::UPPER_ARM_LENGTH],filterFac);
		Fubi::calculateBodyMeasurement(m_currentTrackingData->jointPositions[handToTake],
			m_currentTrackingData->jointPositions[elbowForHand], m_bodyMeasurements[BodyMeasurement::LOWER_ARM_LENGTH],filterFac);
		m_bodyMeasurements[BodyMeasurement::ARM_LENGTH].m_dist = m_bodyMeasurements[BodyMeasurement::LOWER_ARM_LENGTH].m_dist + m_bodyMeasurements[BodyMeasurement::UPPER_ARM_LENGTH].m_dist;
		m_bodyMeasurements[BodyMeasurement::ARM_LENGTH].m_confidence = minf(m_bodyMeasurements[BodyMeasurement::LOWER_ARM_LENGTH].m_confidence, m_bodyMeasurements[BodyMeasurement::UPPER_ARM_LENGTH].m_confidence);

		// Leg lengths
		Fubi::calculateBodyMeasurement(m_currentTrackingData->jointPositions[hipToTake],
			m_currentTrackingData->jointPositions[kneeForHip], m_bodyMeasurements[BodyMeasurement::UPPER_LEG_LENGTH],filterFac);
		Fubi::calculateBodyMeasurement(m_currentTrackingData->jointPositions[footToTake],
			m_currentTrackingData->jointPositions[kneeForFoot], m_bodyMeasurements[BodyMeasurement::LOWER_LEG_LENGTH],filterFac);
		m_bodyMeasurements[BodyMeasurement::LEG_LENGTH].m_dist = m_bodyMeasurements[BodyMeasurement::LOWER_LEG_LENGTH].m_dist + m_bodyMeasurements[BodyMeasurement::UPPER_LEG_LENGTH].m_dist;
		m_bodyMeasurements[BodyMeasurement::LEG_LENGTH].m_confidence = minf(m_bodyMeasurements[BodyMeasurement::LOWER_LEG_LENGTH].m_confidence, m_bodyMeasurements[BodyMeasurement::UPPER_LEG_LENGTH].m_confidence);
	}
}

void FubiUser::reset()
{
	m_isTracked = false;
	m_inScene = false;
	m_id = 0;
	m_lastRightFingerDetection = -1;
	m_lastLeftFingerDetection = -1;
	m_lastBodyMeasurementUpdate = 0;
}

Fubi::RecognitionResult::Result FubiUser::getRecognitionProgress(Combinations::Combination combinationID, std::vector<TrackingData>* userStates,
	bool restart, bool returnFilteredData, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	if(m_combinationRecognizers[combinationID])
		return m_combinationRecognizers[combinationID]->getRecognitionProgress(userStates, restart, returnFilteredData, correctionHint);
	return Fubi::RecognitionResult::NOT_RECOGNIZED;
}

Fubi::RecognitionResult::Result FubiUser::getRecognitionProgress(const std::string& recognizerName, std::vector<TrackingData>* userStates,
	bool restart, bool returnFilteredData, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	std::map<string, CombinationRecognizer*>::iterator rec = m_userDefinedCombinationRecognizers.find(recognizerName);
	if (rec != m_userDefinedCombinationRecognizers.end() && rec->second)
		return rec->second->getRecognitionProgress(userStates, restart, returnFilteredData, correctionHint);
	return Fubi::RecognitionResult::NOT_RECOGNIZED;
}

int FubiUser::getCurrentRecognitionState(const std::string& recognizerName, unsigned int& numStates, bool& isInterrupted, bool& isInTransition)
{
	std::map<string, CombinationRecognizer*>::iterator rec = m_userDefinedCombinationRecognizers.find(recognizerName);
	if (rec != m_userDefinedCombinationRecognizers.end() && rec->second)
	{
		CombinationRecognizer* r = rec->second;
		numStates = r->getNumStates();
		isInterrupted = r->isInterrupted();
		isInTransition = r->isWaitingForTransition();
		return r->getCurrentState();
	}
	return -3;
}

void FubiUser::update()
{
	// Calculate local transformations out of the global ones
	calculateLocalTransformations(m_currentTrackingData->jointPositions, m_currentTrackingData->jointOrientations, m_currentTrackingData->localJointPositions, m_currentTrackingData->localJointOrientations);

	// Filter the current data
	calculateFilteredTransformations();

	// And calculate the local filtered ones
	calculateLocalTransformations(m_filteredTrackingData->jointPositions, m_filteredTrackingData->jointOrientations, m_filteredTrackingData->localJointPositions, m_filteredTrackingData->localJointOrientations);

	// Update body measurements (out of the local transformations)
	updateBodyMeasurements();
						
	// Immediately update the posture combination recognizers (Only if new joint data is here)
	updateCombinationRecognizers();

	// Check and update finger detection
	updateFingerCount();
}