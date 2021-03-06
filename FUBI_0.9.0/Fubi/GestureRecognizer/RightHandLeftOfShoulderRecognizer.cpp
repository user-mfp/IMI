// ****************************************************************************************
//
// Posture Recognizers
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************
#include "RightHandLeftOfShoulderRecognizer.h"

using namespace Fubi;


Fubi::RecognitionResult::Result RightHandLeftOfShoulderRecognizer::recognizeOn(FubiUser* user, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	FubiUser::TrackingData* data = user->m_currentTrackingData;
	if (m_useFilteredData)
		data = user->m_filteredTrackingData;
	const SkeletonJointPosition& rightHand = data->jointPositions[SkeletonJoint::RIGHT_HAND];
	const SkeletonJointPosition& rightShoulder = data->jointPositions[SkeletonJoint::RIGHT_SHOULDER];
	if (rightHand.m_confidence >= m_minConfidence && rightShoulder.m_confidence >= m_minConfidence)
	{
		if (rightHand.m_position.x < rightShoulder.m_position.x)
			return Fubi::RecognitionResult::RECOGNIZED;
	}
	else
		return Fubi::RecognitionResult::TRACKING_ERROR;
	return Fubi::RecognitionResult::NOT_RECOGNIZED;
}