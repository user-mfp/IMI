// ****************************************************************************************
//
// Fubi Leap Sensor
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************
#pragma once

#include "FubiLeapSensor.h"

#ifdef FUBI_USE_LEAP

#include <LeapMath.h>

#ifdef _DEBUG
#pragma comment(lib, "Leapd.lib")
#else
#pragma comment(lib, "Leap.lib")
#endif

using namespace Fubi;

FubiLeapSensor::FubiLeapSensor() : m_hasNewData(false)
{
	m_type = FingerSensorType::LEAP;
}

FubiLeapSensor::~FubiLeapSensor()
{
}

void FubiLeapSensor::update()
{
	if (getController().isConnected())
	{
		int64_t lastID = m_currFrame.id();
		m_currFrame = getController().frame();
		m_hasNewData = lastID != m_currFrame.id();

		// update current ids, delete unused ones
		for (auto oldHandIter = m_fingerIdMap.begin(); oldHandIter != m_fingerIdMap.cend();)
		{
			bool foundHand = false;
			int handID = oldHandIter->first;

			if (m_currFrame.isValid())
			{
				for (int h = 0; h < m_currFrame.hands().count(); ++h)
				{
					const Leap::Hand& hand = m_currFrame.hands()[h];
					if (hand.isValid() && hand.id() == handID)
					{
						// found hand
						foundHand = true;

						// Now update/delete old fingers
						for (int i = 0; i < SkeletonHandJoint::NUM_JOINTS-1; ++i)
						{
							bool foundFinger = false;
							int fingerID = oldHandIter->second.ids[i];
							if (fingerID != -1)
							{
								for (int f = 0; f < hand.fingers().count(); ++f)
								{
									if (hand.fingers()[f].isValid() && hand.fingers()[f].id() == fingerID)
									{
										foundFinger = true;
										break;
									}
								}					

								if (!foundFinger)
								{
									oldHandIter->second.ids[i] = -1;
								}
							}
						}

						// And add new fingers
						for (int f = 0; f < hand.fingers().count(); ++f)
						{
							const Leap::Finger& finger = hand.fingers()[f];
							if (finger.isValid())
							{
								int newID = finger.id();
								bool foundFinger = false;
								int firstFreeSlot = -1;
								for (int i=0; i < SkeletonHandJoint::NUM_JOINTS-1; ++i)
								{
									if (oldHandIter->second.ids[i] == newID)
										foundFinger = true;
									else if (oldHandIter->second.ids[i] == -1 && firstFreeSlot == -1)
										firstFreeSlot = i;
								}
								if (firstFreeSlot == -1)
									break; // No more slots left
								if (!foundFinger)
									oldHandIter->second.ids[firstFreeSlot] = newID;
							}
						}

						break;
					}
				}
			}
			if (!foundHand)
			{
				auto iterToErase = oldHandIter++;
				m_fingerIdMap.erase(iterToErase);
			}
			else
				++oldHandIter;
		}
		// Add new hands
		if (m_currFrame.isValid())
		{
			for (int h = 0; h < m_currFrame.hands().count(); ++h)
			{
				const Leap::Hand& hand = m_currFrame.hands()[h];
				int handId = hand.id();
				auto mapEntry = m_fingerIdMap.find(handId);
				if (mapEntry == m_fingerIdMap.end())
				{			
					FingerIDs newIds;
					int f = 0;
					// Set new ids
					for (; f < hand.fingers().count() && f < SkeletonHandJoint::NUM_JOINTS-1; ++f)
					{
						if (hand.fingers()[f].isValid())
							newIds.ids[f] = hand.fingers()[f].id();
					}
					// invalidate missing ones
					for (;f<SkeletonHandJoint::NUM_JOINTS-1; ++f)
						newIds.ids[f] = -1;

					m_fingerIdMap[handId] = newIds;
				}
			}
		}
	}
}

unsigned short FubiLeapSensor::getHandIDs(unsigned int* handIDs)
{
	int numHands = 0;
	if (m_currFrame.isValid())
	{
		numHands = m_currFrame.hands().count();
		if (handIDs)
		{
			for (int i = 0; i < numHands; ++i)
			{
				handIDs[i] = m_currFrame.hands()[i].id();
			}
		}
	}
	return numHands;
}

bool FubiLeapSensor::isTracking(int id)
{
	if (m_currFrame.isValid())
	{
		const Leap::Hand& h = m_currFrame.hand(id);
		return h.isValid();
	}
	return false;
}

void FubiLeapSensor::getFingerTrackingData(int handID, SkeletonHandJoint::Joint joint,
	Fubi::SkeletonJointPosition& position, Fubi::SkeletonJointOrientation& orientation)
{
	bool fingerFound = false;
	if (m_currFrame.isValid())
	{
		const Leap::Hand& h = m_currFrame.hand(handID);
		if (h.isValid())
		{
			Leap::Vector pos;
			float roll, yaw, pitch;
		
			if (joint == SkeletonHandJoint::PALM)
			{
				fingerFound = true;
				pos = h.palmPosition();
				roll = h.direction().roll();
				yaw = h.direction().yaw();
				pitch = h.direction().pitch();
			}
			else
			{
				auto fingerIds = m_fingerIdMap.find(h.id());
				if (fingerIds != m_fingerIdMap.end())
				{
					int fingerID = (*fingerIds).second.ids[((int)joint)-1];
					if (fingerID != -1)
					{
						const Leap::Finger& f = h.finger(fingerID);
						if (f.isValid())
						{
							fingerFound = true;
							pos = f.tipPosition();
							roll = f.direction().roll();
							yaw = f.direction().yaw();
							pitch = f.direction().pitch();
						}
					}
				}
			}

			if (fingerFound)
			{
				position.m_confidence = 1.0f;
				position.m_position.x = pos.x + m_offsetPos.x;
				position.m_position.y = pos.y + m_offsetPos.y;
				position.m_position.z = pos.z + m_offsetPos.z;

				orientation.m_confidence = 1.0f;
				orientation.m_orientation = Matrix3f::RotMat(pitch, -yaw, -(roll+Math::Pi));
			}
		}
	}

	if (!fingerFound)
	{
		// not found so reset the confidence
		position.m_confidence = 0;
		orientation.m_confidence = 0;
	}
}

int FubiLeapSensor::getFingerCount(int id)
{
	if (m_currFrame.isValid())
	{
		const Leap::Hand& h = m_currFrame.hand(id);
		if (h.isValid())
		{
			return h.fingers().count();
		}
	}
	return -1;
}

bool FubiLeapSensor::init(Fubi::Vec3f offsetPos)
{
	m_offsetPos  = offsetPos;
	// Actually nothing to do here for a real initialization...
	Fubi_logInfo("FubiLeapSensor: succesfully initialized!\n");
	return true;
}
#endif