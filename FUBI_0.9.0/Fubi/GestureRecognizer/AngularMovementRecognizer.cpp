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

#include "AngularMovementRecognizer.h"


using namespace Fubi;

AngularMovementRecognizer::AngularMovementRecognizer(Fubi::SkeletonJoint::Joint joint, const Fubi::Vec3f& minAngularVelocity /*= Fubi::DefaultMinVec*/,
		const Fubi::Vec3f& maxAngularVelocity /*= Fubi::DefaultMaxVec*/,
		bool useLocalOrients /*= true*/, float minConfidence /*= -1.0f*/, bool useFilteredData /*= false*/)
		: m_joint(joint), m_minAngularVelocity(minAngularVelocity), m_maxAngularVelocity(maxAngularVelocity), m_useLocalOrients(useLocalOrients),
	  IGestureRecognizer(false, minConfidence, useFilteredData)
{
	// TODO: use dir/minVel/maxVel to get better hints?
	m_jointUsableForFingerTracking = m_joint < SkeletonHandJoint::NUM_JOINTS;
}


Fubi::RecognitionResult::Result AngularMovementRecognizer::recognizeOn(FubiUser* user, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	Fubi::RecognitionResult::Result result = Fubi::RecognitionResult::NOT_RECOGNIZED;
	
	if (user != 0x0)
	{
		// Get joint positions
		FubiUser::TrackingData* data = user->m_currentTrackingData;
		FubiUser::TrackingData* lastData = user->m_lastTrackingData;
		if (m_useFilteredData)
		{
			data = user->m_filteredTrackingData;
			lastData = user->m_lastFilteredTrackingData;
		}

		float movTime;
		Vec3f angularMovement(Math::NO_INIT);
		if (calcAngularMovement(lastData, data, angularMovement, movTime))
		{
			Vec3f angularVel = angularMovement / movTime;
			// Check if velocity per axis is in between the boundaries
			if (angularVel.x >= m_minAngularVelocity.x && angularVel.x <= m_maxAngularVelocity.x
				&& angularVel.y >= m_minAngularVelocity.y && angularVel.y <= m_maxAngularVelocity.y
				&& angularVel.z >= m_minAngularVelocity.z && angularVel.z <= m_maxAngularVelocity.z)
				result = RecognitionResult::RECOGNIZED;
			else if (correctionHint)
			{
				// Wrong speed
				correctionHint->m_joint = m_joint;
				correctionHint->m_isAngle = true;
				correctionHint->m_changeType = RecognitionCorrectionHint::SPEED;
				correctionHint->m_changeDirection = RecognitionCorrectionHint::DIFFERENT;
					
				if (angularVel.x < m_minAngularVelocity.x)
					correctionHint->m_dirX = m_minAngularVelocity.x-angularVel.x;
				else if (angularVel.x > m_maxAngularVelocity.x)
					correctionHint->m_dirX = m_maxAngularVelocity.x-angularVel.x;
				if (angularVel.y < m_minAngularVelocity.y)
					correctionHint->m_dirY = m_minAngularVelocity.y-angularVel.y;
				else if (angularVel.y > m_maxAngularVelocity.y)
					correctionHint->m_dirY = m_maxAngularVelocity.y-angularVel.y;
				if (angularVel.z < m_minAngularVelocity.z)
					correctionHint->m_dirZ = m_minAngularVelocity.z-angularVel.z;
				else if (angularVel.z > m_maxAngularVelocity.z)
					correctionHint->m_dirZ = m_maxAngularVelocity.z-angularVel.z;
				
			}
		}
		else
			result = Fubi::RecognitionResult::TRACKING_ERROR;
	}

	return result;
}

bool AngularMovementRecognizer::calcAngularMovement(FubiUser::TrackingData* start, FubiUser::TrackingData* end, Vec3f& angularMovement, float& movementTime)
{
	SkeletonJointOrientation* endJoint = &(end->jointOrientations[m_joint]);
	SkeletonJointOrientation* startJoint = &(start->jointOrientations[m_joint]);
	if (m_useLocalOrients)
	{
		endJoint = &(end->localJointOrientations[m_joint]);
		startJoint = &(start->localJointOrientations[m_joint]);
	}

	if (calcAngularMovement(startJoint, endJoint, angularMovement))
	{
		// Ret: prev thing missing is the time difference (do never return zero here!)
		movementTime = clamp(float(end->timeStamp - start->timeStamp), Math::Epsilon, Math::MaxFloat);
		return true;
	}
	return false;
}

bool AngularMovementRecognizer::calcAngularMovement(SkeletonJointOrientation* startJoint, SkeletonJointOrientation* endJoint, Fubi::Vec3f& angularMovement)
{
	// Check confidence
	if (endJoint->m_confidence >= m_minConfidence && startJoint->m_confidence >= m_minConfidence)
	{
		// Calculate orientation of current and prev frame		
		Vec3f vector = endJoint->m_orientation.getRot();
		Vec3f prevVector = startJoint->m_orientation.getRot();	
		
		// Get the difference between both vectors
		angularMovement = vector - prevVector;	
		return true;
	}
	// The confidence values have been too low at some point
	return false;
}

Fubi::RecognitionResult::Result AngularMovementRecognizer::recognizeOn(FubiHand* hand, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	Fubi::RecognitionResult::Result result = Fubi::RecognitionResult::NOT_RECOGNIZED;
	
	if (m_jointUsableForFingerTracking)
	{
		// Get joint positions
		FubiHand::FingerTrackingData* data = hand->m_currentFingerTrackingData;
		FubiHand::FingerTrackingData* lastData = hand->m_lastFingerTrackingData;
		if (m_useFilteredData)
		{
			data = hand->m_filteredFingerTrackingData;
			lastData = hand->m_lastFilteredFingerTrackingData;
		}

		float movTime;
		Vec3f angularMovement(Math::NO_INIT);
		if (calcAngularMovement(lastData, data, angularMovement, movTime))
		{
			Vec3f angularVel = angularMovement / movTime;
			// Check if velocity per axis is in between the boundaries
			if (angularVel.x >= m_minAngularVelocity.x && angularVel.x <= m_maxAngularVelocity.x
				&& angularVel.y >= m_minAngularVelocity.y && angularVel.y <= m_maxAngularVelocity.y
				&& angularVel.z >= m_minAngularVelocity.z && angularVel.z <= m_maxAngularVelocity.z)
				result = RecognitionResult::RECOGNIZED;
			else if (correctionHint)
			{
				// Wrong speed
				correctionHint->m_joint = m_joint;
				correctionHint->m_isAngle = true;
				correctionHint->m_changeType = RecognitionCorrectionHint::SPEED;
				correctionHint->m_changeDirection = RecognitionCorrectionHint::DIFFERENT;
					
				if (angularVel.x < m_minAngularVelocity.x)
					correctionHint->m_dirX = m_minAngularVelocity.x-angularVel.x;
				else if (angularVel.x > m_maxAngularVelocity.x)
					correctionHint->m_dirX = m_maxAngularVelocity.x-angularVel.x;
				if (angularVel.y < m_minAngularVelocity.y)
					correctionHint->m_dirY = m_minAngularVelocity.y-angularVel.y;
				else if (angularVel.y > m_maxAngularVelocity.y)
					correctionHint->m_dirY = m_maxAngularVelocity.y-angularVel.y;
				if (angularVel.z < m_minAngularVelocity.z)
					correctionHint->m_dirZ = m_minAngularVelocity.z-angularVel.z;
				else if (angularVel.z > m_maxAngularVelocity.z)
					correctionHint->m_dirZ = m_maxAngularVelocity.z-angularVel.z;
				
			}
		}
		else
			result = Fubi::RecognitionResult::TRACKING_ERROR;
	}

	return result;
}

bool AngularMovementRecognizer::calcAngularMovement(FubiHand::FingerTrackingData* start, FubiHand::FingerTrackingData* end, Vec3f& angularMovement, float& movementTime)
{
	SkeletonJointOrientation* endJoint = &(end->jointOrientations[m_joint]);
	SkeletonJointOrientation* startJoint = &(start->jointOrientations[m_joint]);
	if (m_useLocalOrients)
	{
		endJoint = &(end->localJointOrientations[m_joint]);
		startJoint = &(start->localJointOrientations[m_joint]);
	}

	if (calcAngularMovement(startJoint, endJoint, angularMovement))
	{
		// Ret: prev thing missing is the time difference (do never return zero here!)
		movementTime = clamp(float(end->timeStamp - start->timeStamp), Math::Epsilon, Math::MaxFloat);
		return true;
	}
	return false;
}