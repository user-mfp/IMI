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

#include "FubiHand.h"

#include "FubiIFingerSensor.h"
#include "FubiCore.h"
#include "FubiRecognizerFactory.h"
#include "GestureRecognizer/CombinationRecognizer.h"

using namespace Fubi;

FubiHand::FubiHand() : m_id(0), m_isTracked(false)
{
	//  Init tracking data
	m_currentFingerTrackingData = new FingerTrackingData();
	m_lastFingerTrackingData = new FingerTrackingData();
	m_filteredFingerTrackingData = new FingerTrackingData();
	m_lastFilteredFingerTrackingData = new FingerTrackingData();

	// And additional init for filtered data
	memset(m_lastFilteredVelocity, 0, sizeof(Fubi::Vec3f)*Fubi::SkeletonHandJoint::NUM_JOINTS);
}


FubiHand::~FubiHand()
{
	clearCombinationRecognizers();

	delete m_currentFingerTrackingData;
	delete m_lastFingerTrackingData;
	delete m_filteredFingerTrackingData;
	delete m_lastFilteredFingerTrackingData;
}

void FubiHand::clearCombinationRecognizers()
{
	std::map<std::string, CombinationRecognizer*>::iterator iter;
	std::map<std::string, CombinationRecognizer*>::iterator end = m_combinationRecognizers.end();
	for (iter = m_combinationRecognizers.begin(); iter != end; ++iter)
	{
		delete iter->second;
	}
	m_combinationRecognizers.clear();
}

void FubiHand::enableCombinationRecognition(const CombinationRecognizer* recognizerTemplate, bool enable)
{
	if (recognizerTemplate)
	{
		std::map<std::string, CombinationRecognizer*>::iterator rec = m_combinationRecognizers.find(recognizerTemplate->getName());
		if (rec != m_combinationRecognizers.end())
		{
			if (enable)
				rec->second->start();
			else
			{
				rec->second->stop();
			}
		}
		else if (enable && recognizerTemplate->getRecognizerTarget() == RecognizerTarget::ALL_SENSORS || recognizerTemplate->getRecognizerTarget() == RecognizerTarget::FINGER_SENSOR)
		{
			CombinationRecognizer* clonedRec = recognizerTemplate->clone();
			clonedRec->setHand(this);
			m_combinationRecognizers[recognizerTemplate->getName()] = clonedRec;
			clonedRec->start();
		}
	}
}

void FubiHand::updateFingerTrackingData(FubiIFingerSensor* sensor)
{
	if (sensor)
	{
		// First update tracking state
		m_isTracked = sensor->isTracking(m_id);

		if (sensor->hasNewTrackingData())
		{
			// Swap tracking data pointers to leave a backup of the current one
			std::swap(m_lastFingerTrackingData, m_currentFingerTrackingData);
			std::swap(m_lastFilteredFingerTrackingData, m_filteredFingerTrackingData);

			// Update timestamp
			m_currentFingerTrackingData->timeStamp = m_filteredFingerTrackingData->timeStamp = Fubi::currentTime();
			// The other joints are only valid if the user is tracked
			if (m_isTracked)
			{
				// Get finger count
				m_currentFingerTrackingData->fingerCount = sensor->getFingerCount(m_id);

				// Get all joint positions/orientations for that hand
				for (unsigned int j=0; j < SkeletonHandJoint::NUM_JOINTS; ++j)
				{
					sensor->getFingerTrackingData(m_id, (SkeletonHandJoint::Joint) j, m_currentFingerTrackingData->jointPositions[j], m_currentFingerTrackingData->jointOrientations[j]);
				}

				update();
			}
		}
	}
}


void FubiHand::updateCombinationRecognizers()
{
	// Update the combination recognizers
	std::map<std::string, CombinationRecognizer*>::iterator iter;
	std::map<std::string, CombinationRecognizer*>::iterator end = m_combinationRecognizers.end();
	for (iter = m_combinationRecognizers.begin(); iter != end; ++iter)
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

void FubiHand::calculateFilteredTransformations()
{
	// Apply filter to current global positions and orientations
	FubiCore* core = FubiCore::getInstance();
	if (core && m_currentFingerTrackingData->timeStamp > m_lastFingerTrackingData->timeStamp)
	{
		// Get filter config from the core
		float cutOffSlope, minCutOff, velocityCutOffFrequency;
		core->getFilterOptions(minCutOff, velocityCutOffFrequency, cutOffSlope);

		// Passed time since last update
		const float timeStep = float(m_currentFingerTrackingData->timeStamp - m_lastFingerTrackingData->timeStamp);

		// Global alpha values for the velocity
		const float velAlpha = oneEuroAlpha(timeStep, velocityCutOffFrequency);
		const float invVelAlpha = 1.0f-velAlpha;

		for (unsigned int i=0; i < SkeletonHandJoint::NUM_JOINTS; ++i)
		{
			// Calculate velocity as position difference..
			Vec3f vel = m_currentFingerTrackingData->jointPositions[i].m_position - m_lastFilteredFingerTrackingData->jointPositions[i].m_position;
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
			m_filteredFingerTrackingData->jointPositions[i].m_position = (m_currentFingerTrackingData->jointPositions[i].m_position*posAlpha) + (m_lastFilteredFingerTrackingData->jointPositions[i].m_position*invPosAlpha);
			// Calculate confidence according to the average alpha value
			float avgAlpha = (posAlpha.x+posAlpha.y+posAlpha.z)/3.0f;
			m_filteredFingerTrackingData->jointPositions[i].m_confidence = avgAlpha*m_currentFingerTrackingData->jointPositions[i].m_confidence + (1.0f-avgAlpha)*m_lastFilteredFingerTrackingData->jointPositions[i].m_confidence;

			// For the orientations, we directly use the min cut off frequency without any velocity dependency
			m_filteredFingerTrackingData->jointOrientations[i].m_orientation = (m_currentFingerTrackingData->jointOrientations[i].m_orientation*minCutOff) + (m_lastFilteredFingerTrackingData->jointOrientations[i].m_orientation*(1.0f-minCutOff));
			// Calculate confidence according to the alpha value
			m_filteredFingerTrackingData->jointOrientations[i].m_confidence = minCutOff*m_currentFingerTrackingData->jointOrientations[i].m_confidence + (1.0f-minCutOff)*m_lastFilteredFingerTrackingData->jointOrientations[i].m_confidence;

			// Only copy the finger count
			m_filteredFingerTrackingData->fingerCount = m_currentFingerTrackingData->fingerCount;
		}
	}
}


void FubiHand::reset()
{
	m_isTracked = false;
	m_id = 0;
}

Fubi::RecognitionResult::Result FubiHand::getRecognitionProgress(const std::string& recognizerName, std::vector<FingerTrackingData>* userStates,
	bool restart, bool returnFilteredData, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	std::map<string, CombinationRecognizer*>::iterator rec = m_combinationRecognizers.find(recognizerName);
	if (rec != m_combinationRecognizers.end() && rec->second)
		return rec->second->getRecognitionProgress(userStates, restart, returnFilteredData, correctionHint);
	return Fubi::RecognitionResult::NOT_RECOGNIZED;
}

int FubiHand::getCurrentRecognitionState(const std::string& recognizerName, unsigned int& numStates, bool& isInterrupted, bool& isInTransition)
{
	std::map<string, CombinationRecognizer*>::iterator rec = m_combinationRecognizers.find(recognizerName);
	if (rec != m_combinationRecognizers.end() && rec->second)
	{
		CombinationRecognizer* r = rec->second;
		numStates = r->getNumStates();
		isInterrupted = r->isInterrupted();
		isInTransition = r->isWaitingForTransition();
		return r->getCurrentState();
	}
	return -3;
}

void FubiHand::update()
{
	// Calculate local transformations out of the global ones
	calculateLocalHandTransformations(m_currentFingerTrackingData->jointPositions, m_currentFingerTrackingData->jointOrientations, m_currentFingerTrackingData->localJointPositions, m_currentFingerTrackingData->localJointOrientations);

	// Filter the current data
	calculateFilteredTransformations();

	// And calculate the local filtered ones
	calculateLocalHandTransformations(m_filteredFingerTrackingData->jointPositions, m_filteredFingerTrackingData->jointOrientations, m_filteredFingerTrackingData->localJointPositions, m_filteredFingerTrackingData->localJointOrientations);
					
	// Immediately update the combination recognizers (Only if new joint data is here?)
	updateCombinationRecognizers();
}