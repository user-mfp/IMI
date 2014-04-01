// ****************************************************************************************
//
// Fubi Linear Acceleration Recognizer
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************

#pragma once

#include "IGestureRecognizer.h"

class LinearAccelerationRecognizer : public IGestureRecognizer
{
public:
	LinearAccelerationRecognizer(Fubi::SkeletonJoint::Joint joint, 
		const Fubi::Vec3f& direction, float minAccel, float maxAccel = Fubi::Math::MaxFloat,
		float minConfidence = -1.0f,
		float maxAngleDiff = 45.0f, bool useOnlyCorrectDirectionComponent = true);

	virtual ~LinearAccelerationRecognizer() {}

	virtual Fubi::RecognitionResult::Result recognizeOn(FubiUser* user, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);

	virtual IGestureRecognizer* clone() { return new LinearAccelerationRecognizer(*this); }

private:
	Fubi::SkeletonJoint::Joint m_joint;
	Fubi::Vec3f m_direction;
	bool m_directionValid;
	bool m_useOnlyCorrectDirectionComponent;
	float m_minAccel;
	float m_maxAccel;
	float m_maxAngleDiff;
};