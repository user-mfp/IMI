// ****************************************************************************************
//
// Fubi Recognizer Factory
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************

#pragma once

// Common interface for recognizers
#include "GestureRecognizer/IGestureRecognizer.h"

#include "FubiUser.h"

using namespace std;

namespace Fubi
{
	// Create a static posture recognizer
	IGestureRecognizer* createPostureRecognizer(Postures::Posture postureID);

	// Create a joint relation recognizer
	IGestureRecognizer* createPostureRecognizer(SkeletonJoint::Joint joint, SkeletonJoint::Joint relJoint,
		const Fubi::Vec3f& minValues = DefaultMinVec, 
		const Fubi::Vec3f& maxValues = DefaultMaxVec, 
		float minDistance = 0, 
		float maxDistance = Fubi::Math::MaxFloat,
		bool useLocalPositions = false,
		float minConfidence = -1.0f,
		Fubi::BodyMeasurement::Measurement measuringUnit = Fubi::BodyMeasurement::NUM_MEASUREMENTS,
		bool useFilteredData = false);

	// Create a joint orientation recognizer
	IGestureRecognizer* createPostureRecognizer(SkeletonJoint::Joint joint,
		const Fubi::Vec3f& minValues = Fubi::Vec3f(-180.0f, -180.0f, -180.0f), 
		const Fubi::Vec3f& maxValues = Fubi::Vec3f(180.0f, 180.0f, 180.0f),
		bool useLocalOrientations = true,
		float minConfidence = -1.0f,
		bool useFilteredData = false);
	IGestureRecognizer* createPostureRecognizer(SkeletonJoint::Joint joint,
		const Fubi::Vec3f& orientation,
		float maxAngleDiff = 45.0f,
		bool useLocalOrientations = true,
		float minConfidence = -1.0f,
		bool useFilteredData = false);

	// Create a linear movement recognizer
	IGestureRecognizer* createMovementRecognizer(SkeletonJoint::Joint joint, SkeletonJoint::Joint relJoint,
		const Fubi::Vec3f& direction, 
		float minVel, float maxVel = Fubi::Math::MaxFloat,
		bool useLocalPositions = false,
		float minConfidence = -1.0f,
		float maxAngleDiff = 45.0f, 
		bool useOnlyCorrectDirectionComponent = true,
		bool useFilteredData = false);
	IGestureRecognizer* createMovementRecognizer(SkeletonJoint::Joint joint,
		const Fubi::Vec3f& direction, 
		float minVel, float maxVel = Fubi::Math::MaxFloat,
		bool useLocalPositions = false,
		float minConfidence = -1.0f,
		float maxAngleDiff = 45.0f, 
		bool useOnlyCorrectDirectionComponent = true,
		bool useFilteredData = false);
	IGestureRecognizer* createMovementRecognizer(SkeletonJoint::Joint joint, SkeletonJoint::Joint relJoint,
		const Fubi::Vec3f& direction, 
		float minVel, float maxVel,
		float minLength, float maxLength = Fubi::Math::MaxFloat,
		Fubi::BodyMeasurement::Measurement measuringUnit = Fubi::BodyMeasurement::NUM_MEASUREMENTS,
		bool useLocalPositions = false,
		float minConfidence = -1.0f,
		float maxAngleDiff = 45.0f, 
		bool useOnlyCorrectDirectionComponent = true,
		bool useFilteredData = false);
	IGestureRecognizer* createMovementRecognizer(SkeletonJoint::Joint joint,
		const Fubi::Vec3f& direction, 
		float minVel, float maxVel,
		float minLength, float maxLength = Fubi::Math::MaxFloat,
		Fubi::BodyMeasurement::Measurement measuringUnit = Fubi::BodyMeasurement::NUM_MEASUREMENTS,
		bool useLocalPositions = false,
		float minConfidence = -1.0f,
		float maxAngleDiff = 45.0f, 
		bool useOnlyCorrectDirectionComponent = true,
		bool useFilteredData = false);

	// Or a angular movement rec
	IGestureRecognizer* createMovementRecognizer(Fubi::SkeletonJoint::Joint joint, const Fubi::Vec3f& minAngularVelocity = Fubi::DefaultMinVec,
		const Fubi::Vec3f& maxAngularVelocity = Fubi::DefaultMaxVec,
		bool useLocalOrients = true, float minConfidence = -1.0f,
		bool useFilteredData = false);

	// Or linear acceleration
	IGestureRecognizer* createMovementRecognizer(Fubi::SkeletonJoint::Joint joint,
		const Fubi::Vec3f& direction, 
		float minAccel, float maxAccel = Fubi::Math::MaxFloat,
		float minConfidence = -1.0f,
		float maxAngleDiff = 45.0f, 
		bool useOnlyCorrectDirectionComponent = true);

	// Create a finger count recognizer
	IGestureRecognizer* createPostureRecognizer(Fubi::SkeletonJoint::Joint handJoint = Fubi::SkeletonJoint::RIGHT_HAND,
		unsigned int minFingers = 0, unsigned int maxFingers = 5,
		float minConfidence = -1.0f, bool useMedianCalculation = false,
		bool useFilteredData = false);

	// Create a posture combination recognizer
	CombinationRecognizer* createCombinationRecognizer(FubiUser* user, Combinations::Combination postureID);
}
