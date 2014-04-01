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
#pragma once

/** \file FubiHand.h 
 * \brief a header file containing the FubiUser class definition
*/ 

#include "FubiPredefinedGestures.h"
#include "FubiMath.h"

#include <map>
#include <deque>
#include <queue>

class CombinationRecognizer;

/** \addtogroup FUBIUSER FUBI User class
* Contains the FubiUser class that holds informations for tracked users
* 
* @{
*/

/**
* \brief The FubiUser class hold all relevant informations for each tracked user
*/
class FubiUser
{
public:
	/**
	* \brief Tracking data with global and local positions/orientations and a timestamp
	*/
	struct TrackingData
	{
		TrackingData() : timeStamp(0) {}
		Fubi::SkeletonJointPosition jointPositions[Fubi::SkeletonJoint::NUM_JOINTS];
		Fubi::SkeletonJointPosition localJointPositions[Fubi::SkeletonJoint::NUM_JOINTS];
		Fubi::SkeletonJointOrientation jointOrientations[Fubi::SkeletonJoint::NUM_JOINTS];
		Fubi::SkeletonJointOrientation localJointOrientations[Fubi::SkeletonJoint::NUM_JOINTS];
		double timeStamp;
	};
	/**
	* \brief Acceleration data and a timestamp
	*/
	struct AccelerationData
	{
		AccelerationData() : timeStamp(0) {}
		Fubi::SkeletonJointAcceleration accelerations[Fubi::SkeletonJoint::NUM_JOINTS];
		double timeStamp;
	};

	/**
	* \brief Constructor
	*  Note: For internal use only!
	*/
	FubiUser();
	/**
	* \brief Destructor
	*  Note: For internal use only!
	*/
	~FubiUser();

	/**
	* \brief Operator used for comparing users according to their distance to the sensor
	* in the x-z plane --> get the closest users
	*/
	static bool closerToSensor(const FubiUser* u1, const FubiUser* u2)
	{
		const Fubi::SkeletonJointPosition& pos1 = u1->m_currentTrackingData->jointPositions[Fubi::SkeletonJoint::TORSO];
		const Fubi::SkeletonJointPosition& pos2 = u2->m_currentTrackingData->jointPositions[Fubi::SkeletonJoint::TORSO];

		if (u1->m_isTracked && pos1.m_confidence > 0.1f)
		{
			if (u2->m_isTracked  && pos2.m_confidence > 0.1f)
			{
				// Compare their distance (int the x,z-plane) to the sensor
				float dist1 = sqrtf(pos1.m_position.z*pos1.m_position.z + pos1.m_position.x*pos1.m_position.x);
				float dist2 = sqrtf(pos2.m_position.z*pos2.m_position.z + pos2.m_position.x*pos2.m_position.x);
				return dist1 < dist2;
			}
			else
			{
				// u1 is "closer" to the sensor (only valid user)
				return true;
			}
		}
		else if (u2->m_isTracked  && pos2.m_confidence > 0.1f)
		{
			return false; // u2 is "closer" to the sensor (only valid user)
		}

		// No valid user -> comparison has no meaning
		// but we compare the id to retain a strict weak ordering
		return u1->m_id < u2->m_id;
	}

	/**
	* \brief Operator used for comparing which user is more left of the sensor
	*/
	static bool moreLeft(const FubiUser* u1, const FubiUser* u2)
	{
		const Fubi::SkeletonJointPosition& pos1 = u1->m_currentTrackingData->jointPositions[Fubi::SkeletonJoint::TORSO];
		const Fubi::SkeletonJointPosition& pos2 = u2->m_currentTrackingData->jointPositions[Fubi::SkeletonJoint::TORSO];

		if (u1->m_isTracked && pos1.m_confidence > 0.1f)
		{
			if (u2->m_isTracked  && pos2.m_confidence > 0.1f)
			{
				// Compare their x value
				return pos1.m_position.x < pos2.m_position.x;
			}
			else
			{
				// u1 is "more left" to the sensor (only valid user)
				return true;
			}
		}
		else if (u2->m_isTracked  && pos2.m_confidence > 0.1f)
		{
			return false; // u2 is "more left" to the sensor (only valid user)
		}

		// No valid user -> comparison has no meaning
		// but we compare the id to retain a strict weak ordering
		return u1->m_id < u2->m_id;
	}

	/**
	* \brief Enables/disables a posture combination recognizer of this user
	* Note: For internal use only, please use Fubi::enableCombinationRecognition() instead!
	*/
	void enableCombinationRecognition(Fubi::Combinations::Combination postureID, bool enable);
	void enableCombinationRecognition(const CombinationRecognizer* recognizerTemplate, bool enable);

	/**
	* \brief Enable/disables the tracking of the shown number of fingers for each hand
	* Note: For internal use only, please use Fubi:enableFingerTracking:() instead!
	*/
	void enableFingerTracking(bool leftHand, bool rightHand, bool useConvexityDefectMethod = false);

	/**
	* \brief Gets the finger count optionally calculated by the median of the last 10 calculations
	* Note: For internal use only, please use Fubi::getFingerCount() instead!
	*/
	int getFingerCount(bool leftHand = false, bool getMedianOfLastFrames = true, bool useOldConvexityDefectMethod = false);

	/**
	* \brief Stops and removes all user defined recognizers
	* Note: For internal use only, please use Fubi::clearUserDefinedRecognizers() instead!
	*/
	void clearUserDefinedCombinationRecognizers();

	/**
	* \brief Calculates filtered and local transformations and updates the combination recognizers
	* Note: For internal use only, please use Fubi::updateSensor() instead!
	*/
	void update();

	/**
	* \brief Update the tracking info from the given sensor
	* Note: For internal use only, please use Fubi::updateSensor() instead!
	*/ 
	void updateTrackingData(class FubiISensor* sensor);

	/**
	* \brief Reset the user to an initial state 
	* Note: For internal use only, please use Fubi::resetTracking() instead!
	*/
	void reset();

	/**
	* \brief Get the recognition progress of a combination recognizer associated to this user
	* Note: For internal use only, please use Fubi::getCombinationRecognitionProgressOn() instead!
	*/
	Fubi::RecognitionResult::Result getRecognitionProgress(Fubi::Combinations::Combination combinationID, std::vector<TrackingData>* userStates,
		bool restart, bool returnFilteredData, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);
	Fubi::RecognitionResult::Result getRecognitionProgress(const std::string& recognizerName, std::vector<TrackingData>* userStates,
		bool restart, bool returnFilteredData, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);
	/**
	* \brief or the current state of the combination recognizer 
	* Note: For internal use only, please use Fubi::getCurrentCombinationRecognitionState() instead!
	*/
	int getCurrentRecognitionState(const std::string& recognizerName, unsigned int& numStates, bool& isInterrupted, bool& isInTransition);

	/**
	* \brief Manually update the tracking info
	* Note: For internal use only, please use Fubi::updateTrackingData() instead!
	*/
	void addNewTrackingData(Fubi::SkeletonJointPosition* positions,
		double timeStamp = -1, Fubi::SkeletonJointOrientation* orientations = 0);

	/**
	* \brief and acceleartion data
	* Note: For internal use only, please use Fubi::updateTrackingData() instead!
	*/
	void addNewAccelerationData(Fubi::SkeletonJointAcceleration* acceleration, double timeStamp = 0);

	/**
	* \brief Get the debug image of the last finger count detection
	* Note: For internal use only!
	*/
	const Fubi::FingerCountImageData* getFingerCountImageData(bool left = false)
	{
		return left ? &m_leftFingerCountImage : &m_rightFingerCountImage;
	}

	/**
	* \brief Whether the user is currently seen in the depth image
	*/
	bool m_inScene;

	/**
	* \brief Id of this user
	*/
	unsigned int m_id;

	/**
	* \brief Whether the user is currently tracked
	*/
	bool m_isTracked;

	/**
	* \brief Current and last, filtered and unfiltered tracking data including joint positions and orientations (both local and global ones)
	*/
	TrackingData *m_currentTrackingData, *m_lastTrackingData, *m_lastFilteredTrackingData, *m_filteredTrackingData;

	/**
	* \brief Acceleration data
	*/
	AccelerationData *m_accelerationData;

	/**
	* \brief The user's body measurements
	*/
	Fubi::BodyMeasurementDistance m_bodyMeasurements[Fubi::BodyMeasurement::NUM_MEASUREMENTS];

private:
	/**
	* \brief Adds a finger count detection to the deque for later median calculation
	*/
	void addFingerCount(int count, bool leftHand = false);

	/**
	* \brief Apply a filter on the tracking data
	*/
	void calculateFilteredTransformations();

	/**
	* \brief Update the combination recognizers according to the current tracking data
	*/
	void updateCombinationRecognizers();

	/**
	* \brief Update the finger count calculation
	*/
	void updateFingerCount();

	/**
	* \brief Calculate the median of the given finger count array
	*/
	int calculateMedianFingerCount(const std::deque<int>& fingerCount);

	/**
	* \brief Update the body measurements out of the currently tracked positions
	*/
	void updateBodyMeasurements();

	/**
	* \brief One posture combination recognizer per posture combination
	*/
	CombinationRecognizer* m_combinationRecognizers[Fubi::Combinations::NUM_COMBINATIONS];
	/**
	* \brief And all user defined ones that are currently enabled
	*/
	std::map<std::string, CombinationRecognizer*> m_userDefinedCombinationRecognizers;

	/**
	* \brief When the last detection of the finger count of each hand happened, -1 if disabled
	*/
	double m_lastRightFingerDetection, m_lastLeftFingerDetection;	
	/**
	* \brief What method to use for the finger count detection
	*/
	bool m_useConvexityDefectMethod;	
	/**
	* \brief Number of last finger count detections to use for median calculation
	*/
	unsigned int m_maxFingerCountForMedian;
	/**
	* \brief Lhe last finger count detections
	*/
	std::deque<int> m_rightFingerCount, m_leftFingerCount;
	/**
	* \brief Time between the finger count detection of one hand
	*/
	double m_fingerTrackIntervall;
	/**
	* \brief Degbug images created during the finger count detection
	*/
	Fubi::FingerCountImageData m_leftFingerCountImage, m_rightFingerCountImage;

	/**
	* \brief Timestamp of the last body measurement update
	*/
	double m_lastBodyMeasurementUpdate;

	/**
	* \brief Additional filtering history for the velocities
	*/
	Fubi::Vec3f m_lastFilteredVelocity[Fubi::SkeletonJoint::NUM_JOINTS];
};

/*! @}*/