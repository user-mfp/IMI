// ****************************************************************************************
//
// Joint Relation Recognizers
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************

#pragma once

#include "IGestureRecognizer.h"

class JointRelationRecognizer : public IGestureRecognizer
{
public:
	// +-MaxFloat are the default values, as they represent no restriction
	JointRelationRecognizer(Fubi::SkeletonJoint::Joint joint, Fubi::SkeletonJoint::Joint relJoint,
		const Fubi::Vec3f& minValues = Fubi::DefaultMinVec, 
		const Fubi::Vec3f& maxValues = Fubi::DefaultMaxVec,
		float minDistance = 0, 
		float maxDistance = Fubi::Math::MaxFloat,
		bool useLocalPositions = false,
		float minConfidence = -1.0f,
		Fubi::BodyMeasurement::Measurement measuringUnit = Fubi::BodyMeasurement::NUM_MEASUREMENTS,
		bool useFilteredData = false);

	virtual ~JointRelationRecognizer() {}

	virtual Fubi::RecognitionResult::Result recognizeOn(FubiUser* user, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);
	virtual Fubi::RecognitionResult::Result recognizeOn(FubiHand* hand, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);

	virtual IGestureRecognizer* clone() { return new JointRelationRecognizer(*this); }

private:
	Fubi::RecognitionResult::Result recognize(Fubi::SkeletonJointPosition* joint, Fubi::SkeletonJointPosition* relJoint, Fubi::BodyMeasurementDistance* measure = 0x0, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);

	Fubi::SkeletonJoint::Joint m_joint;
	Fubi::SkeletonJoint::Joint m_relJoint;
	bool m_jointsUsableForHandTracking;
	Fubi::Vec3f m_minValues, m_maxValues;
	float m_minDistance;
	float m_maxDistance;
	bool m_useLocalPositions;
	Fubi::BodyMeasurement::Measurement m_measuringUnit;
};