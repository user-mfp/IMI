// ****************************************************************************************
//
// Fubi Angular Movement Recognizer
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************

#pragma once

#include "IGestureRecognizer.h"

class AngularMovementRecognizer : public IGestureRecognizer
{
public:
	AngularMovementRecognizer(Fubi::SkeletonJoint::Joint joint, const Fubi::Vec3f& minAngularVelocity = Fubi::DefaultMinVec,
		const Fubi::Vec3f& maxAngularVelocity = Fubi::DefaultMaxVec,
		bool useLocalOrients = true, float minConfidence = -1.0f,
		bool useFilteredData = false);

	virtual ~AngularMovementRecognizer() {}

	virtual Fubi::RecognitionResult::Result recognizeOn(FubiUser* user, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);
	virtual Fubi::RecognitionResult::Result recognizeOn(FubiHand* hand, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);

	virtual IGestureRecognizer* clone() { return new AngularMovementRecognizer(*this); }

private:
	bool calcAngularMovement(FubiUser::TrackingData* start, FubiUser::TrackingData* end, Fubi::Vec3f& angularMovement, float& movementTime);
	bool calcAngularMovement(FubiHand::FingerTrackingData* start, FubiHand::FingerTrackingData* end, Fubi::Vec3f& angularMovement, float& movementTime);
	bool calcAngularMovement(Fubi::SkeletonJointOrientation* startJoint, Fubi::SkeletonJointOrientation* endJoint, Fubi::Vec3f& angularMovement);

	Fubi::SkeletonJoint::Joint m_joint;
	bool m_jointUsableForFingerTracking;
	Fubi::Vec3f m_minAngularVelocity;
	Fubi::Vec3f m_maxAngularVelocity;
	bool m_useLocalOrients;
};