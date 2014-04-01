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
#include "JointRelationRecognizer.h"

using namespace Fubi;

JointRelationRecognizer::JointRelationRecognizer(Fubi::SkeletonJoint::Joint joint, Fubi::SkeletonJoint::Joint relJoint, 
	const Fubi::Vec3f& minValues /*= Fubi::DefaultMinVec*/, 
	const Fubi::Vec3f& maxValues /*= Fubi::DefaultMaxVec*/, 
	float minDistance /*= 0*/, 
	float maxDistance /*= Fubi::Math::MaxFloat*/,
	bool useLocalPositions /*=false*/,
	float minConfidence /*= -1.0f*/,
	Fubi::BodyMeasurement::Measurement measuringUnit /*= Fubi::BodyMeasurement::NUM_MEASUREMENTS*/,
	bool useFilteredData /*= false*/)
	: m_joint(joint), m_relJoint(relJoint),
	  m_minValues(minValues), m_maxValues(maxValues), m_minDistance(minDistance), m_maxDistance(maxDistance), 
	  m_useLocalPositions(useLocalPositions),
	  m_measuringUnit(measuringUnit),
	  IGestureRecognizer(false, minConfidence, useFilteredData)
{
	// It is actually a bit hacky to use the skeleton joint enum for the hand joint values, but it saves a lot of work...
	m_jointsUsableForHandTracking = m_joint < SkeletonHandJoint::NUM_JOINTS && (m_relJoint < SkeletonHandJoint::NUM_JOINTS || m_relJoint == SkeletonJoint::NUM_JOINTS);
}

Fubi::RecognitionResult::Result JointRelationRecognizer::recognize(SkeletonJointPosition* joint, SkeletonJointPosition* relJoint, Fubi::BodyMeasurementDistance* measure /*=0x0*/, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	bool recognized = false;
	if (joint->m_confidence >= m_minConfidence)
	{
		bool vecValid = false;
		Vec3f vector(Fubi::Math::NO_INIT);
		
		if (relJoint)
		{
			vecValid = relJoint->m_confidence >= m_minConfidence;
			if(vecValid)
			{
				vector = joint->m_position - relJoint->m_position;
			}
		}
		else
		{
			vector = joint->m_position;
			vecValid = true;
		}
			
		if (vecValid && measure != 0x0)
		{
			if (measure->m_confidence >= m_minConfidence	&& measure->m_dist > Math::Epsilon)
				vector /= measure->m_dist;
			else
				vecValid = false;
		}

		if (vecValid)
		{			

			float distance = vector.length();
		
			bool xInRange = vector.x >= m_minValues.x && vector.x <= m_maxValues.x;
			bool yInRange = vector.y >= m_minValues.y && vector.y <= m_maxValues.y;
			bool zInRange = vector.z >= m_minValues.z && vector.z <= m_maxValues.z;

			bool distInRange = distance >= m_minDistance && distance <= m_maxDistance;

			recognized = xInRange && yInRange && zInRange && distInRange;

			if (!recognized && correctionHint)
			{
				correctionHint->m_changeType = RecognitionCorrectionHint::LENGTH;
				correctionHint->m_joint = m_joint;
				correctionHint->m_isAngle = false;
				correctionHint->m_changeDirection = RecognitionCorrectionHint::DIFFERENT;

				if (vector.x < m_minValues.x)
					correctionHint->m_dirX = m_minValues.x - vector.x;
				else if(vector.x > m_maxValues.x)
					correctionHint->m_dirX = m_maxValues.x - vector.x;
				if (vector.y < m_minValues.y)
					correctionHint->m_dirY = m_minValues.y - vector.y;
				else if(vector.y > m_maxValues.y)
					correctionHint->m_dirY = m_maxValues.y - vector.y;
				if (vector.z < m_minValues.z)
					correctionHint->m_dirZ = m_minValues.z - vector.z;
				else if(vector.z > m_maxValues.z)
					correctionHint->m_dirZ = m_maxValues.z - vector.z;				
			}
		}
		else
			return Fubi::RecognitionResult::TRACKING_ERROR;
	}
	else
		return Fubi::RecognitionResult::TRACKING_ERROR;

	return recognized ? Fubi::RecognitionResult::RECOGNIZED : Fubi::RecognitionResult::NOT_RECOGNIZED;
}

Fubi::RecognitionResult::Result JointRelationRecognizer::recognizeOn(FubiUser* user, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	FubiUser::TrackingData* data = user->m_currentTrackingData;
	if (m_useFilteredData)
		data = user->m_filteredTrackingData;

	SkeletonJointPosition* joint = &(data->jointPositions[m_joint]);
	if (m_useLocalPositions)
		joint = &(data->localJointPositions[m_joint]);
	SkeletonJointPosition* relJoint = 0x0;
	if (m_relJoint != Fubi::SkeletonJoint::NUM_JOINTS)
	{
		relJoint = &(data->jointPositions[m_relJoint]);
		if (m_useLocalPositions)
			relJoint = &(data->localJointPositions[m_relJoint]);
	}
	BodyMeasurementDistance* measure = 0x0;
	if (m_measuringUnit != BodyMeasurement::NUM_MEASUREMENTS)
		measure = &(user->m_bodyMeasurements[m_measuringUnit]);

	return recognize(joint, relJoint, measure, correctionHint);
}

Fubi::RecognitionResult::Result JointRelationRecognizer::recognizeOn(FubiHand* hand, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	if (m_jointsUsableForHandTracking)
	{
		FubiHand::FingerTrackingData* data = hand->m_currentFingerTrackingData;
		if (m_useFilteredData)
			data = hand->m_filteredFingerTrackingData;

		SkeletonJointPosition* joint = &(data->jointPositions[m_joint]);
		if (m_useLocalPositions)
			joint = &(data->localJointPositions[m_joint]);
		SkeletonJointPosition* relJoint = 0x0;
		if (m_relJoint != Fubi::SkeletonJoint::NUM_JOINTS)
		{
			relJoint = &(data->jointPositions[m_relJoint]);
			if (m_useLocalPositions)
				relJoint = &(data->localJointPositions[m_relJoint]);
		}
		return recognize(joint, relJoint, 0x0, correctionHint);
	}
	return Fubi::RecognitionResult::NOT_RECOGNIZED;
}