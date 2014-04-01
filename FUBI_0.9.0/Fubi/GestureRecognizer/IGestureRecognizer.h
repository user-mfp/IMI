// ****************************************************************************************
//
// Fubi Gesture Recognizer
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************

#pragma once

#include "../FubiUser.h"
#include "../FubiHand.h"

#include <map>

class IGestureRecognizer
{
public:
	IGestureRecognizer() : m_ignoreOnTrackingError(false), m_minConfidence(0.51f), m_useFilteredData(false), m_targetSensor(Fubi::RecognizerTarget::BODY_SENSOR) {}
	IGestureRecognizer(bool ignoreOnTrackingError, float minconfidence, bool useFilteredData)
		: m_ignoreOnTrackingError(ignoreOnTrackingError), m_useFilteredData(useFilteredData), m_targetSensor(Fubi::RecognizerTarget::BODY_SENSOR)
	{ m_minConfidence = (minconfidence >= 0) ? minconfidence : 0.51f; }
	virtual ~IGestureRecognizer() {}

	// Method signatures for testing the recognizer on a specfic user/hand, need to be overwritten by sub classes to apply the recognition
	virtual Fubi::RecognitionResult::Result recognizeOn(FubiUser* user, Fubi::RecognitionCorrectionHint* correctionHint = 0x0)
	{ return Fubi::RecognitionResult::NOT_RECOGNIZED; }
	virtual Fubi::RecognitionResult::Result recognizeWithHistory(FubiUser* user, FubiUser::TrackingData* initialData, FubiUser::TrackingData* initialFilteredData, Fubi::RecognitionCorrectionHint* correctionHint = 0x0)
	{ return Fubi::RecognitionResult::NOT_RECOGNIZED; }
	virtual Fubi::RecognitionResult::Result recognizeOn(FubiHand* hand, Fubi::RecognitionCorrectionHint* correctionHint = 0x0)
	{ return Fubi::RecognitionResult::NOT_RECOGNIZED; }	
	virtual Fubi::RecognitionResult::Result recognizeWithHistory(FubiHand* hand, FubiHand::FingerTrackingData* initialData, FubiHand::FingerTrackingData* initialFilteredData, Fubi::RecognitionCorrectionHint* correctionHint = 0x0)
	{ return Fubi::RecognitionResult::NOT_RECOGNIZED; }

	virtual bool useHistory() { return false; }

	virtual IGestureRecognizer* clone() = 0;

	bool m_ignoreOnTrackingError;
	float m_minConfidence;
	bool m_useFilteredData;
	Fubi::RecognizerTarget::Target m_targetSensor;
};