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

#include "LinearAccelerationRecognizer.h"


using namespace Fubi;

LinearAccelerationRecognizer::LinearAccelerationRecognizer(Fubi::SkeletonJoint::Joint joint,
		const Fubi::Vec3f& direction, float minAccel, float maxAccel /*= Fubi::Math::MaxFloat*/,
		float minConfidence /*= -1.0f*/, float maxAngleDiff /*= 45.0f*/, 
		bool useOnlyCorrectDirectionComponent /*= true*/)
	: m_joint(joint),  m_minAccel(minAccel), m_maxAccel(maxAccel),
	  m_maxAngleDiff(maxAngleDiff), m_useOnlyCorrectDirectionComponent(useOnlyCorrectDirectionComponent),
	  IGestureRecognizer(false, minConfidence, false)
{
	m_directionValid = direction.length() > Math::Epsilon;
	if (m_directionValid)
		m_direction = direction.normalized();
}

Fubi::RecognitionResult::Result LinearAccelerationRecognizer::recognizeOn(FubiUser* user, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	Fubi::RecognitionResult::Result result = Fubi::RecognitionResult::NOT_RECOGNIZED;
	
	if (user != 0x0)
	{
		// Get joint positions
		FubiUser::AccelerationData* data = user->m_accelerationData;
		SkeletonJointAcceleration* joint = &(data->accelerations[m_joint]);

		// Check confidence
		if (joint->m_confidence >= m_minConfidence)
		{					
			float accel = 0;
			float angleDiff = 0;
			float accelLength = joint->m_acceleration.length();
			if (m_directionValid)
			{
				if (m_useOnlyCorrectDirectionComponent)
				{
					// Weight the vector components according to the given direction
					// Apply the direction stretched to the same length on the vector
					// Components in the correct direction will result in a positive value
					// Components in the wrong direction have a negative value
					Vec3f dirVector = joint->m_acceleration * (m_direction * accelLength);			
					// Build the sum of the weighted and signed components
					float sum = dirVector.x + dirVector.y + dirVector.z;
					// Calcluate the velocity (if there are too many negative values it may be less then zero)
					accel = (sum <= 0) ? (-sqrt(-sum)) : (sqrt(sum));
				}
				else
					// calculate the velocity directly from the current vector
					accel = accelLength;
				// Additionally check the angle difference
				angleDiff = radToDeg(acosf(joint->m_acceleration.dot(m_direction) / accelLength));
			}
			else
			{
				// No direction given so check for movement speed in any direction
				accel = accelLength;
			}
			// Check if velocity is in between the boundaries
			if (accel >= m_minAccel && accel <= m_maxAccel && angleDiff <= m_maxAngleDiff)
				result = RecognitionResult::RECOGNIZED;
		}
		else
			result = Fubi::RecognitionResult::TRACKING_ERROR;
	}
	return result;
}