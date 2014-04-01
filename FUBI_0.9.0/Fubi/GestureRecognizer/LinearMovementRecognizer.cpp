// ****************************************************************************************
//
// Fubi Linear Movement Recognizer
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************

#include "LinearMovementRecognizer.h"


using namespace Fubi;

LinearMovementRecognizer::LinearMovementRecognizer(Fubi::SkeletonJoint::Joint joint, Fubi::SkeletonJoint::Joint relJoint, 
		const Fubi::Vec3f& direction, float minVel, float maxVel /*= Fubi::Math::MaxFloat*/, bool useLocalPos /*= false*/,
		float minConfidence /*= -1.0f*/, float maxAngleDiff /*= 45.0f*/, 
		bool useOnlyCorrectDirectionComponent /*= true*/,
		bool useFilteredData /*= false*/)
	: m_joint(joint), m_relJoint(relJoint), m_useRelJoint(true),
	  m_minVel(minVel), m_maxVel(maxVel), m_useLocalPos(useLocalPos),
	  m_maxAngleDiff(maxAngleDiff), m_useOnlyCorrectDirectionComponent(useOnlyCorrectDirectionComponent),
	  m_lengthValid(false),
	  IGestureRecognizer(false, minConfidence, useFilteredData)
{
	m_jointsUsableForFingerTracking = m_joint < SkeletonHandJoint::NUM_JOINTS && (m_relJoint < SkeletonHandJoint::NUM_JOINTS || m_relJoint == SkeletonJoint::NUM_JOINTS);
	m_directionValid = direction.length() > Math::Epsilon;
	if (m_directionValid)
		m_direction = direction.normalized();
}

LinearMovementRecognizer::LinearMovementRecognizer(Fubi::SkeletonJoint::Joint joint,
		const Fubi::Vec3f& direction, float minVel, float maxVel /*= Fubi::Math::MaxFloat*/,
		bool useLocalPos /*= false*/, float minConfidence /*= -1.0f*/, float maxAngleDiff /*= 45.0f*/, 
		bool useOnlyCorrectDirectionComponent /*= true*/,
		bool useFilteredData /*= false*/)
	: m_joint(joint), m_useRelJoint(false),
	m_minVel(minVel), m_maxVel(maxVel), m_useLocalPos(useLocalPos),
	m_maxAngleDiff(maxAngleDiff), m_useOnlyCorrectDirectionComponent(useOnlyCorrectDirectionComponent),
	m_lengthValid(false),
	IGestureRecognizer(false, minConfidence, useFilteredData)
{
	m_jointsUsableForFingerTracking = m_joint < SkeletonHandJoint::NUM_JOINTS && (m_relJoint < SkeletonHandJoint::NUM_JOINTS || m_relJoint == SkeletonJoint::NUM_JOINTS);
	m_directionValid = direction.length() > Math::Epsilon;
	if (m_directionValid)
		m_direction = direction.normalized();
}

LinearMovementRecognizer::LinearMovementRecognizer(Fubi::SkeletonJoint::Joint joint, Fubi::SkeletonJoint::Joint relJoint, 
		const Fubi::Vec3f& direction, float minVel, float maxVel,
		float minLength, float maxLength /*= Fubi::Math::MaxFloat*/,
		Fubi::BodyMeasurement::Measurement measuringUnit /*= Fubi::BodyMeasurement::NUM_MEASUREMENTS*/,
		bool useLocalPos /*= false*/, float minConfidence /*= -1.0f*/,
		float maxAngleDiff /*= 45.0f*/, bool useOnlyCorrectDirectionComponent /*= true*/,
		bool useFilteredData /*= false*/)
	: m_joint(joint), m_relJoint(relJoint), m_useRelJoint(true),
	  m_minVel(minVel), m_maxVel(maxVel), m_useLocalPos(useLocalPos),
	  m_minLength(minLength), m_maxLength(maxLength),
	  m_measuringUnit(measuringUnit),
	  m_maxAngleDiff(maxAngleDiff), m_useOnlyCorrectDirectionComponent(useOnlyCorrectDirectionComponent),
	  m_lengthValid(true),
	  IGestureRecognizer(false, minConfidence, useFilteredData)
{
	m_jointsUsableForFingerTracking = m_joint < SkeletonHandJoint::NUM_JOINTS && (m_relJoint < SkeletonHandJoint::NUM_JOINTS || m_relJoint == SkeletonJoint::NUM_JOINTS);
	m_directionValid = direction.length() > Math::Epsilon;
	if (m_directionValid)
		m_direction = direction.normalized();
}

LinearMovementRecognizer::LinearMovementRecognizer(Fubi::SkeletonJoint::Joint joint, const Fubi::Vec3f& direction,
		float minVel, float maxVel,
		float minLength, float maxLength /*= Fubi::Math::MaxFloat*/,
		Fubi::BodyMeasurement::Measurement measuringUnit /*= Fubi::BodyMeasurement::NUM_MEASUREMENTS*/,
		bool useLocalPos /*= false*/, float minConfidence /*= -1.0f*/,
		float maxAngleDiff /*= 45.0f*/, bool useOnlyCorrectDirectionComponent /*= true*/,
		bool useFilteredData /*= false*/)
	: m_joint(joint), m_useRelJoint(false),
	m_minVel(minVel), m_maxVel(maxVel), m_useLocalPos(useLocalPos),
	m_minLength(minLength), m_maxLength(maxLength),
	m_measuringUnit(measuringUnit),
	m_maxAngleDiff(maxAngleDiff), m_useOnlyCorrectDirectionComponent(useOnlyCorrectDirectionComponent),
	m_lengthValid(true),
	IGestureRecognizer(false, minConfidence, useFilteredData)
{
	m_jointsUsableForFingerTracking = m_joint < SkeletonHandJoint::NUM_JOINTS && (m_relJoint < SkeletonHandJoint::NUM_JOINTS || m_relJoint == SkeletonJoint::NUM_JOINTS);
	m_directionValid = direction.length() > Math::Epsilon;
	if (m_directionValid)
		m_direction = direction.normalized();
}

Fubi::RecognitionResult::Result LinearMovementRecognizer::recognizeOn(FubiUser* user, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
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

		float movLength, movTime, angleDiff;
		Vec3f movement(Math::NO_INIT);
		if (calcMovement(lastData, data, movLength, movTime, angleDiff, movement))
		{
			float vel = movLength / movTime;
			// Check if velocity is in between the boundaries and angle diff is low enough
			if (angleDiff <= m_maxAngleDiff)
			{
				if (vel >= m_minVel && vel <= m_maxVel)
					result = RecognitionResult::RECOGNIZED;
				else if (correctionHint)
				{
					// Wrong speed
					correctionHint->m_joint = m_joint;
					correctionHint->m_isAngle = false;
					correctionHint->m_changeType = RecognitionCorrectionHint::SPEED;
					
					float correctionSpeed;
					if (vel < m_minVel)
					{
						correctionSpeed = m_minVel-vel;
						correctionHint->m_changeDirection = RecognitionCorrectionHint::MORE;
					}
					else
					{
						correctionSpeed = m_maxVel-vel;
						correctionHint->m_changeDirection = RecognitionCorrectionHint::LESS;
					}
					Vec3f correction = m_directionValid ? (m_direction*correctionSpeed) : Vec3f(correctionSpeed, 0, 0);
					correctionHint->m_dirX = correction.x;
					correctionHint->m_dirY = correction.y;
					correctionHint->m_dirZ = correction.z;
				}
			}
			else if (correctionHint)
			{
				// Wrong direction
				correctionHint->m_joint = m_joint;
				correctionHint->m_isAngle = false;
				correctionHint->m_changeType = RecognitionCorrectionHint::DIRECTION;
				correctionHint->m_changeDirection = RecognitionCorrectionHint::DIFFERENT;
				Vec3f diff = m_direction*m_minVel/*-(movement/movLength)*/;
				correctionHint->m_dirX = diff.x;
				correctionHint->m_dirY = diff.y;
				correctionHint->m_dirZ = diff.z;
			}

			//if (/*!recognized && */abs(vel) > 200)
			//{
			//	if (m_maxVel > 10000.0f)
			//	{
			//		Fubi_logInfo("Lin Gesture rec: vel=%4.0f <= %4.0f <= INF recognized=%s\n", 
			//		  m_minVel, vel, (result == RecognitionResult::RECOGNIZED) ? "true" : "false");
			//	}
			//	else
			//		Fubi_logInfo("Lin Gesture rec: vel=%4.0f <= %4.0f <= %4.0f recognized=%s\n", 
			//		m_minVel, vel, m_maxVel, (result == RecognitionResult::RECOGNIZED) ? "true" : "false");
			//	/*diffVector.normalize();

			//	Fubi_logInfo("Lin Gesture rec: Hand.z=%.3f, targetDir=%.3f/%.3f/%.3f \n\t\tactualDir=%.3f/%.3f/%.3f vel=%.0f/%.0f recognized=%s\n", 
			//		joint.m_position.z,
			//		m_direction.x, m_direction.y, m_direction.z, 
			//		diffVector.x, diffVector.y, diffVector.z,
			//		vel, m_minVel, recognized ? "true" : "false");*/
			//}
		}
		else
			result = Fubi::RecognitionResult::TRACKING_ERROR;
	}

	return result;
}

Fubi::RecognitionResult::Result LinearMovementRecognizer::recognizeWithHistory(FubiUser* user, FubiUser::TrackingData* initialData, FubiUser::TrackingData* initialFilteredData, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	Fubi::RecognitionResult::Result result = Fubi::RecognitionResult::NOT_RECOGNIZED;

	FubiUser::TrackingData* startData = m_useFilteredData ? initialData : initialFilteredData;

	if (startData == 0x0)
	{
		// We don't have a start point yet, so just look for the current direction
		if (recognizeOn(user) == RecognitionResult::RECOGNIZED)
			result = RecognitionResult::WAITING_FOR_LAST_STATE_TO_FINISH;
	}
	else if (user != 0x0)
	{
		// Get joint positions
		FubiUser::TrackingData* endData = user->m_currentTrackingData;
		FubiUser::TrackingData* lastData = user->m_lastTrackingData;
		if (m_useFilteredData)
		{
			endData = user->m_filteredTrackingData;
			lastData = user->m_lastFilteredTrackingData;
		}

		float curLength, curTime, curAnglediff, vel = 0;
		Vec3f curMovement(Math::NO_INIT);
		if (calcMovement(lastData, endData, curLength, curTime, curAnglediff, curMovement))
		{
			// use current data only for calculating the current velocity
			// max angle diff only used for the full path vector
			vel = curLength / curTime;

			if (vel >= m_minVel && vel <= m_maxVel /*&& curAnglediff <= m_maxAngleDiff*/)
			{
				// Current velocity is in bounds, now check for the full path vector
				float movLength, movTime, angleDiff;
				Vec3f movement(Math::NO_INIT);
				if (calcMovement(startData, endData, movLength, movTime, angleDiff, movement))
				{
					// Check if movement is in between the boundaries
					if (angleDiff <= m_maxAngleDiff)
					{
						if (movLength < m_minLength)
						{
							result = RecognitionResult::WAITING_FOR_LAST_STATE_TO_FINISH;
							if (correctionHint)
							{
								// Still in progress, but give permanent feedback
								correctionHint->m_joint = m_joint;
								correctionHint->m_isAngle = false;
								correctionHint->m_changeType = RecognitionCorrectionHint::LENGTH;
								correctionHint->m_changeDirection = RecognitionCorrectionHint::MORE;
								Vec3f diff = m_directionValid ? ((m_direction*m_minLength)-movement) : (movement/movLength*(m_minLength-movLength));
								correctionHint->m_dirX = diff.x;
								correctionHint->m_dirY = diff.y;
								correctionHint->m_dirZ = diff.z;
							}
						}
						else if (movLength <= m_maxLength)
						{
							result = RecognitionResult::RECOGNIZED;
						}
						else if (correctionHint)
						{
							// Too far
							correctionHint->m_joint = m_joint;
							correctionHint->m_isAngle = false;
							correctionHint->m_changeType = RecognitionCorrectionHint::LENGTH;
							correctionHint->m_changeDirection = RecognitionCorrectionHint::LESS;
							Vec3f diff = m_directionValid ? ((m_direction*m_maxLength)-movement) : (movement/movLength*(m_maxLength-movLength));
							correctionHint->m_dirX = diff.x;
							correctionHint->m_dirY = diff.y;
							correctionHint->m_dirZ = diff.z;
						}
					}
					else if (correctionHint)
					{
						// Wrong direction
						correctionHint->m_joint = m_joint;
						correctionHint->m_isAngle = false;
						correctionHint->m_changeType = RecognitionCorrectionHint::DIRECTION;
						correctionHint->m_changeDirection = RecognitionCorrectionHint::DIFFERENT;
						Vec3f diff = m_direction*m_minVel/*-(movement/movLength)*/;
						correctionHint->m_dirX = diff.x;
						correctionHint->m_dirY = diff.y;
						correctionHint->m_dirZ = diff.z;
					}
				}
				else
					result = Fubi::RecognitionResult::TRACKING_ERROR;
			}
			else if (correctionHint)
			{
				// Wrong speed
				correctionHint->m_joint = m_joint;
				correctionHint->m_isAngle = false;
				correctionHint->m_changeType = RecognitionCorrectionHint::SPEED;
					
				float correctionSpeed;
				if (vel < m_minVel)
				{
					correctionSpeed = m_minVel-vel;
					correctionHint->m_changeDirection = RecognitionCorrectionHint::MORE;
				}
				else
				{
					correctionSpeed = m_maxVel-vel;
					correctionHint->m_changeDirection = RecognitionCorrectionHint::LESS;
				}
				Vec3f correction = m_directionValid ? (m_direction*correctionSpeed) : Vec3f(correctionSpeed, 0, 0);
				correctionHint->m_dirX = correction.x;
				correctionHint->m_dirY = correction.y;
				correctionHint->m_dirZ = correction.z;
			}
		}
		else
			result = Fubi::RecognitionResult::TRACKING_ERROR;
	}

	return result;
}

bool LinearMovementRecognizer::calcMovement(FubiUser::TrackingData* start, FubiUser::TrackingData* end,
	float& movementLength, float& movementTime, float& directionAngleDiff, Fubi::Vec3f& movement)
{
	SkeletonJointPosition* endJoint = &(end->jointPositions[m_joint]);
	SkeletonJointPosition* startJoint = &(start->jointPositions[m_joint]);
	if (m_useLocalPos)
	{
		endJoint = &(end->localJointPositions[m_joint]);
		startJoint = &(start->localJointPositions[m_joint]);
	}

	SkeletonJointPosition* startRelJoint = 0x0;
	SkeletonJointPosition* endRelJoint = 0x0;
	if (m_useRelJoint)
	{
		startRelJoint = &(start->jointPositions[m_relJoint]);
		endRelJoint = &(end->jointPositions[m_relJoint]);
		if (m_useLocalPos)
		{
			startRelJoint = &(start->localJointPositions[m_relJoint]);
			endRelJoint = &(end->localJointPositions[m_relJoint]);
		}
	}

	if (calcMovement(startJoint, endJoint, startRelJoint, endRelJoint, movementLength, directionAngleDiff, movement))
	{
		// Ret: prev thing missing is the time difference (do never return zero here!)
		movementTime = clamp(float(end->timeStamp - start->timeStamp), Math::Epsilon, Math::MaxFloat);
		return true;
	}
	
	// The confidence values have been too low at some point
	return false;
}

bool LinearMovementRecognizer::calcMovement(SkeletonJointPosition* startJoint, SkeletonJointPosition* endJoint, Fubi::SkeletonJointPosition* startRelJoint, Fubi::SkeletonJointPosition* endRelJoint,
	float& movementLength, float& directionAngleDiff, Fubi::Vec3f& movement)
{
	// Check confidence
	if (endJoint->m_confidence >= m_minConfidence && startJoint->m_confidence >= m_minConfidence)
	{
		bool relJointsValid = false;

		// Calculate relative vector of start and end frame
		Vec3f endVector(Fubi::Math::NO_INIT);
		Vec3f startVector(Fubi::Math::NO_INIT);
		if (m_useRelJoint)
		{
			relJointsValid = startRelJoint->m_confidence >= m_minConfidence && endRelJoint->m_confidence >= m_minConfidence;
			if(relJointsValid)
			{
				endVector = endJoint->m_position - endRelJoint->m_position;
				startVector = startJoint->m_position - startRelJoint->m_position;
			}
		}
		else
		{
			// Absolute values (relative to sensor position)
			relJointsValid = true;
			endVector = endJoint->m_position;
			startVector = startJoint->m_position;
		}
	
		
		if (relJointsValid)
		{
			// Get the difference between both vectors
			movement = endVector - startVector;	

			if (m_directionValid)
			{
				if (m_useOnlyCorrectDirectionComponent)
				{
					// Weight the vector components according to the given direction
					// Apply the direction stretched to the same length on the vector
					// Components in the correct direction will result in a positive value
					// Components in the wrong direction have a negative value
					Vec3f dirVector = movement * (m_direction * movement.length());
			
					// Build the sum of the weighted and signed components
					float sum = dirVector.x + dirVector.y + dirVector.z;

					// Ret: Calcluate the movement length (if there are too many negative values it may be less then zero)
					movementLength = (sum <= 0) ? -sqrt(-sum)  : sqrt(sum);

					// Ret: angle difference
					directionAngleDiff = radToDeg(acosf(movement.dot(m_direction) / (movement.length())));
				}
				else
				{
					// Ret: Calculate the movement directly from the current vector
					movementLength = movement.length();

					// Ret: angle difference
					directionAngleDiff = radToDeg(acosf(movement.dot(m_direction) / movementLength));
				}
			}
			else
			{
				// Ret: No direction given so check for movement in any direction
				movementLength = movement.length();
				directionAngleDiff = 0;
			}

			return true;
		}
	}
	return false;
}

Fubi::RecognitionResult::Result LinearMovementRecognizer::recognizeOn(FubiHand* hand, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	Fubi::RecognitionResult::Result result = Fubi::RecognitionResult::NOT_RECOGNIZED;

	if (m_jointsUsableForFingerTracking)
	{
		// Get joint positions
		FubiHand::FingerTrackingData* data = hand->m_currentFingerTrackingData;
		FubiHand::FingerTrackingData* lastData = hand->m_lastFingerTrackingData;
		if (m_useFilteredData)
		{
			data = hand->m_filteredFingerTrackingData;
			lastData = hand->m_lastFilteredFingerTrackingData;
		}

		float movLength, movTime, angleDiff;
		Vec3f movement(Math::NO_INIT);
		if (calcMovement(lastData, data, movLength, movTime, angleDiff, movement))
		{
			float vel = movLength / movTime;
			// Check if velocity is in between the boundaries and angle diff is low enough
			if (angleDiff <= m_maxAngleDiff)
			{
				if (vel >= m_minVel && vel <= m_maxVel)
					result = RecognitionResult::RECOGNIZED;
				else if (correctionHint)
				{
					// Wrong speed
					correctionHint->m_joint = m_joint;
					correctionHint->m_isAngle = false;
					correctionHint->m_changeType = RecognitionCorrectionHint::SPEED;
					float correctionSpeed;
					if (vel < m_minVel)
					{
						correctionSpeed = m_minVel-vel;
						correctionHint->m_changeDirection = RecognitionCorrectionHint::MORE;
					}
					else
					{
						correctionSpeed = m_maxVel-vel;
						correctionHint->m_changeDirection = RecognitionCorrectionHint::LESS;
					}
					Vec3f correction = m_directionValid ? (m_direction*correctionSpeed) : Vec3f(correctionSpeed, 0, 0);
					correctionHint->m_dirX = correction.x;
					correctionHint->m_dirY = correction.y;
					correctionHint->m_dirZ = correction.z;
				}
			}
			else if (correctionHint)
			{
				// Wrong direction
				correctionHint->m_joint = m_joint;
				correctionHint->m_isAngle = false;
				correctionHint->m_changeType = RecognitionCorrectionHint::DIRECTION;
				correctionHint->m_changeDirection = RecognitionCorrectionHint::DIFFERENT;
				Vec3f diff = m_direction-(movement/movLength);
				correctionHint->m_dirX = diff.x;
				correctionHint->m_dirY = diff.y;
				correctionHint->m_dirZ = diff.z;
			}
		}
		else
			result = Fubi::RecognitionResult::TRACKING_ERROR;
	}

	return result;
}

Fubi::RecognitionResult::Result LinearMovementRecognizer::recognizeWithHistory(FubiHand* hand, FubiHand::FingerTrackingData* initialData, FubiHand::FingerTrackingData* initialFilteredData, Fubi::RecognitionCorrectionHint* correctionHint /*= 0x0*/)
{
	Fubi::RecognitionResult::Result result = Fubi::RecognitionResult::NOT_RECOGNIZED;

	FubiHand::FingerTrackingData* startData = m_useFilteredData ? initialData : initialFilteredData;

	if (startData == 0x0)
	{
		// We don't have a start point yet, so just look for the current direction
		if (recognizeOn(hand) == RecognitionResult::RECOGNIZED)
			result = RecognitionResult::WAITING_FOR_LAST_STATE_TO_FINISH;
	}
	else if (m_jointsUsableForFingerTracking)
	{
		// Get joint positions
		FubiHand::FingerTrackingData* endData = hand->m_currentFingerTrackingData;
		FubiHand::FingerTrackingData* lastData = hand->m_lastFingerTrackingData;
		if (m_useFilteredData)
		{
			endData = hand->m_filteredFingerTrackingData;
			lastData = hand->m_lastFilteredFingerTrackingData;
		}

		float curLength, curTime, curAnglediff, vel = 0;
		Vec3f movement(Math::NO_INIT);
		if (calcMovement(lastData, endData, curLength, curTime, curAnglediff, movement))
		{
			// use current data only for calculating the current velocity
			// max angle diff only used for the full path vector
			vel = curLength / curTime;

			if (vel >= m_minVel && vel <= m_maxVel /*&& curAnglediff <= m_maxAngleDiff*/)
			{
				// Current velocity is in bounds, now check for the full path vector
				float movLength, movTime, angleDiff;
				if (calcMovement(startData, endData, movLength, movTime, angleDiff, movement))
				{
					// Check if movement is in between the boundaries
					if (angleDiff <= m_maxAngleDiff)
					{
						if (movLength < m_minLength)
						{
							result = RecognitionResult::WAITING_FOR_LAST_STATE_TO_FINISH;
							if (correctionHint)
							{
								// Still in progress, but give permanent feedback
								correctionHint->m_joint = m_joint;
								correctionHint->m_isAngle = false;
								correctionHint->m_changeType = RecognitionCorrectionHint::LENGTH;
								correctionHint->m_changeDirection = RecognitionCorrectionHint::MORE;
								Vec3f diff = m_directionValid ? ((m_direction*m_minLength)-movement) : (movement/movLength*(m_minLength-movLength));
								correctionHint->m_dirX = diff.x;
								correctionHint->m_dirY = diff.y;
								correctionHint->m_dirZ = diff.z;
							}
						}
						else if (movLength <= m_maxLength)
						{
							result = RecognitionResult::RECOGNIZED;
						}
						else if (correctionHint)
						{
							// Too far
							correctionHint->m_joint = m_joint;
							correctionHint->m_isAngle = false;
							correctionHint->m_changeType = RecognitionCorrectionHint::LENGTH;
							correctionHint->m_changeDirection = RecognitionCorrectionHint::LESS;
							Vec3f diff = m_directionValid ? ((m_direction*m_maxLength)-movement) : (movement/movLength*(m_maxLength-movLength));
							correctionHint->m_dirX = diff.x;
							correctionHint->m_dirY = diff.y;
							correctionHint->m_dirZ = diff.z;
						}
					}
					else if (correctionHint)
					{
						// Wrong direction
						correctionHint->m_joint = m_joint;
						correctionHint->m_isAngle = false;
						correctionHint->m_changeType = RecognitionCorrectionHint::DIRECTION;
						correctionHint->m_changeDirection = RecognitionCorrectionHint::DIFFERENT;
						Vec3f diff = m_direction-(movement/movLength);
						correctionHint->m_dirX = diff.x;
						correctionHint->m_dirY = diff.y;
						correctionHint->m_dirZ = diff.z;
					}
				}
				else
					result = Fubi::RecognitionResult::TRACKING_ERROR;
			}
			else if (correctionHint)
			{
				// Wrong speed
				correctionHint->m_joint = m_joint;
				correctionHint->m_isAngle = false;
				correctionHint->m_changeType = RecognitionCorrectionHint::SPEED;
					
				float correctionSpeed;
				if (vel < m_minVel)
				{
					correctionSpeed = m_minVel-vel;
					correctionHint->m_changeDirection = RecognitionCorrectionHint::MORE;
				}
				else
				{
					correctionSpeed = m_maxVel-vel;
					correctionHint->m_changeDirection = RecognitionCorrectionHint::LESS;
				}
				Vec3f correction = m_directionValid ? (m_direction*correctionSpeed) : Vec3f(correctionSpeed, 0, 0);
				correctionHint->m_dirX = correction.x;
				correctionHint->m_dirY = correction.y;
				correctionHint->m_dirZ = correction.z;
			}
		}
		else
			result = Fubi::RecognitionResult::TRACKING_ERROR;
	}

	return result;
}

bool LinearMovementRecognizer::calcMovement(FubiHand::FingerTrackingData* start, FubiHand::FingerTrackingData* end,
	float& movementLength, float& movementTime, float& directionAngleDiff, Vec3f& movement)
{
	SkeletonJointPosition* endJoint = &(end->jointPositions[m_joint]);
	SkeletonJointPosition* startJoint = &(start->jointPositions[m_joint]);
	if (m_useLocalPos)
	{
		endJoint = &(end->localJointPositions[m_joint]);
		startJoint = &(start->localJointPositions[m_joint]);
	}
	SkeletonJointPosition* startRelJoint = 0x0;
	SkeletonJointPosition* endRelJoint = 0x0;
	if (m_useRelJoint)
	{
		startRelJoint = &(start->jointPositions[m_relJoint]);
		endRelJoint = &(end->jointPositions[m_relJoint]);
		if (m_useLocalPos)
		{
			startRelJoint = &(start->localJointPositions[m_relJoint]);
			endRelJoint = &(end->localJointPositions[m_relJoint]);
		}
	}

	if (calcMovement(startJoint, endJoint, startRelJoint, endRelJoint, movementLength, directionAngleDiff, movement))
	{
		// Ret: only thing missing is the time difference (do never return zero here!)
		movementTime = clamp(float(end->timeStamp - start->timeStamp), Math::Epsilon, Math::MaxFloat);
		return true;
	}
	
	// The confidence values have been too low at some point
	return false;
}