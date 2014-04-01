// ****************************************************************************************
//
// Finger Count Recognizer
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************

#pragma once

#include "IGestureRecognizer.h"

class FingerCountRecognizer : public IGestureRecognizer
{
public:
	FingerCountRecognizer(Fubi::SkeletonJoint::Joint handJoint = Fubi::SkeletonJoint::RIGHT_HAND,
		unsigned int minFingers = 0, unsigned int maxFingers = 5, float minConfidence = -1.0f, bool useMedianCalculation = false,
		bool useFilteredData = false);

	virtual ~FingerCountRecognizer();

	virtual Fubi::RecognitionResult::Result recognizeOn(FubiUser* user, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);
	virtual Fubi::RecognitionResult::Result recognizeOn(FubiHand* hand, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);

	virtual IGestureRecognizer* clone();

	int getLastFingerCount() {return m_lastRecognition;}

private:
	Fubi::SkeletonJoint::Joint m_handJoint;
	int m_minFingers, m_maxFingers;
	int m_lastRecognition;
	bool m_useMedianCalculation;
};