// ****************************************************************************************
//
// Finger Count Recognizers
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************
#include "FingerCountRecognizer.h"
#include "../FubiImageProcessing.h"
#include "../Fubi.h"

using namespace Fubi;

FingerCountRecognizer::FingerCountRecognizer(Fubi::SkeletonJoint::Joint handJoint /*= Fubi::SkeletonJoint::RIGHT_HAND*/,
		unsigned int minFingers /*= 0*/, unsigned int maxFingers /*= 5*/, float minConfidence /*= -1.0f*/, bool useMedianCalculation /*= false*/,
	bool useFilteredData /*= false*/)
	: m_handJoint(handJoint), m_minFingers(minFingers), m_maxFingers(maxFingers), m_lastRecognition(-1), m_useMedianCalculation(useMedianCalculation),
	IGestureRecognizer(false, minConfidence, useFilteredData)
{
}

FingerCountRecognizer::~FingerCountRecognizer()
{
}

IGestureRecognizer* FingerCountRecognizer::clone()
{
	return new FingerCountRecognizer(m_handJoint, m_minFingers, m_maxFingers, m_minConfidence, m_useMedianCalculation);
}

Fubi::RecognitionResult::Result FingerCountRecognizer::recognizeOn(FubiUser* user, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	bool leftHand = (m_handJoint == Fubi::SkeletonJoint::LEFT_HAND);

	FubiUser::TrackingData* data = user->m_currentTrackingData;
	if (m_useFilteredData)
		data = user->m_filteredTrackingData;
	
	if (data->jointPositions[m_handJoint].m_confidence >= m_minConfidence)
	{
		m_lastRecognition = Fubi::getFingerCount(user->m_id, leftHand, m_useMedianCalculation);
		/*m_lastRecognition = user->getFingerCount(leftHand);*/
		if (m_lastRecognition > -1 && m_lastRecognition >= m_minFingers && m_lastRecognition <= m_maxFingers)
			return Fubi::RecognitionResult::RECOGNIZED;
		else
		{
			if (correctionHint)
			{
				correctionHint->m_joint = m_handJoint;
				if (m_lastRecognition < 0)
				{
					correctionHint->m_dirX = (float)m_minFingers;
					correctionHint->m_changeDirection = RecognitionCorrectionHint::MORE;
				}
				else
				{
					if (m_lastRecognition < m_minFingers)
					{
						correctionHint->m_dirX = (float)(m_minFingers-m_lastRecognition);
						correctionHint->m_changeDirection = RecognitionCorrectionHint::MORE;
					}
					else
					{
						correctionHint->m_dirX = (float)(m_maxFingers-m_lastRecognition);
						correctionHint->m_changeDirection = RecognitionCorrectionHint::LESS;
					}
				}
				correctionHint->m_isAngle = false;
				correctionHint->m_changeType = RecognitionCorrectionHint::FINGERS;
			}
			return Fubi::RecognitionResult::NOT_RECOGNIZED;
		}
	}
	else
		return Fubi::RecognitionResult::TRACKING_ERROR;
}

Fubi::RecognitionResult::Result FingerCountRecognizer::recognizeOn(FubiHand* hand, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	FubiHand::FingerTrackingData* data = hand->m_currentFingerTrackingData;
	if (m_useFilteredData)
		data = hand->m_filteredFingerTrackingData;
	
	if (hand->m_isTracked)
	{
		m_lastRecognition = data->fingerCount;
		if (m_lastRecognition > -1 && m_lastRecognition >= m_minFingers && m_lastRecognition <= m_maxFingers)
			return Fubi::RecognitionResult::RECOGNIZED;
		else
		{
			if (correctionHint)
			{
				if (m_lastRecognition < 0)
				{
					correctionHint->m_dirX = (float)m_minFingers;
					correctionHint->m_changeDirection = RecognitionCorrectionHint::MORE;
				}
				else
				{
					if (m_lastRecognition < m_minFingers)
					{
						correctionHint->m_dirX = (float)(m_minFingers-m_lastRecognition);
						correctionHint->m_changeDirection = RecognitionCorrectionHint::MORE;
					}
					else
					{
						correctionHint->m_dirX = (float)(m_maxFingers-m_lastRecognition);
						correctionHint->m_changeDirection = RecognitionCorrectionHint::LESS;
					}
				}
				correctionHint->m_isAngle = false;
				correctionHint->m_changeType = RecognitionCorrectionHint::FINGERS;
			}
			return Fubi::RecognitionResult::NOT_RECOGNIZED;
		}
	}
	else
		return Fubi::RecognitionResult::TRACKING_ERROR;
}