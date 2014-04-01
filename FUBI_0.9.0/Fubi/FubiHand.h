// ****************************************************************************************
//
// Fubi Hand
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************
#pragma once

/** \file FubiHand.h 
 * \brief a header file containing the FubiHand class definition
*/ 

#include "FubiUtils.h"
#include "FubiMath.h"

#include <map>
#include <deque>
#include <queue>

class CombinationRecognizer;

/** \addtogroup FUBIHAND FUBI Hand class
* Contains the FubiHand class that holds informations for tracked hands
* 
* @{
*/

/**
 * \brief The FubiHand class holds all relevant informations for each tracked hand
 */
class FubiHand
{
public:
	/**
	 * \brief Tracking data with global and local positions/orientations, the current finger count and a timestamp
	 */
	struct FingerTrackingData
	{
		FingerTrackingData() : timeStamp(0) {}
		Fubi::SkeletonJointPosition jointPositions[Fubi::SkeletonHandJoint::NUM_JOINTS];
		Fubi::SkeletonJointOrientation jointOrientations[Fubi::SkeletonHandJoint::NUM_JOINTS];
		Fubi::SkeletonJointPosition localJointPositions[Fubi::SkeletonHandJoint::NUM_JOINTS];
		Fubi::SkeletonJointOrientation localJointOrientations[Fubi::SkeletonHandJoint::NUM_JOINTS];
		int fingerCount;
		double timeStamp;
	};

	/**
	 * \brief Constructor for FubiHand
	 * Note: For internal use only!
	 */
	FubiHand();
	/**
	 * \brief Destructor for FubiHand
	 * Note: For internal use only!
	 */
	~FubiHand();

	/**
	 * \brief Enables/disables a posture combination recognizer of this user
	 * Note: For internal use only, please use Fubi::enableCombinationRecognition() instead!
	 */
	void enableCombinationRecognition(const CombinationRecognizer* recognizerTemplate, bool enable);

	/**
	 * \brief Stops and removes all combination recognizers
	 * Note: For internal use only, please use Fubi::clearUserDefinedRecognizers() instead!
	 */
	void clearCombinationRecognizers();

	/**
	 * \brief Calculates filtered and local transformations and updates the combination recognizers
	 * Note: For internal use only, please use Fubi::updateSensor() instead!
	 */
	void update();

	/**
	 * \brief Update the tracking info from the given sensor
	 * Note: For internal use only, please use Fubi::updateSensor() instead!
	 */
	void updateFingerTrackingData(class FubiIFingerSensor* sensor);

	/**
	 * \brief Reset the user to an initial state
	 * Note: For internal use only, please use Fubi::resetTracking() instead!
	 */
	void reset();

	/**
	 * \brief Get the recognition progress of a combination recognizer associated to this user
	 * Note: For internal use only, please use Fubi::getCombinationRecognitionProgressOn() instead!
	 */
	Fubi::RecognitionResult::Result getRecognitionProgress(const std::string& recognizerName, std::vector<FingerTrackingData>* userStates,
		bool restart, bool returnFilteredData, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);
	/**
	 * \brief Get the  the current state of the combination recognizer
	 * Note: For internal use only, please use Fubi::getCurrentCombinationRecognitionState() instead!
	 */
	int getCurrentRecognitionState(const std::string& recognizerName, unsigned int& numStates, bool& isInterrupted, bool& isInTransition);

	/**
	 * \brief Id of this user
	 */
	unsigned int m_id;

	/**
	 * \brief Whether the hand is currently tracked
	 */
	bool m_isTracked;
	
	/**
	 * \brief Current and last, filtered and unfiltered tracking data including joint positions and orientations (both local and global ones)
	 */
	FingerTrackingData *m_currentFingerTrackingData, *m_lastFingerTrackingData, *m_lastFilteredFingerTrackingData, *m_filteredFingerTrackingData;

private:
	/**
	 * \brief Apply a filter on the tracking data
	 */
	void calculateFilteredTransformations();

	/**
	 * \brief Update the combination recognizers according to the current tracking data
	 */
	void updateCombinationRecognizers();

	/**
	 * \brief All enabled user defined combination recognizers
	 */
	std::map<std::string, CombinationRecognizer*> m_combinationRecognizers;

	/**
	 * \brief Additional filtering history for the velocities
	 */
	Fubi::Vec3f m_lastFilteredVelocity[Fubi::SkeletonHandJoint::NUM_JOINTS];
};

/*! @}*/