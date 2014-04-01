// ****************************************************************************************
//
// Posture Recognizers
// ---------------------------------------------------------
// Copyright (C) 2010-2013 Felix Kistler 
// 
// This software is distributed under the terms of the Eclipse Public License v1.0.
// A copy of the license may be obtained at: http://www.eclipse.org/org/documents/epl-v10.html
// 
// ****************************************************************************************

#pragma once

#include "IGestureRecognizer.h"

class RightHandCloseToArmRecognizer : public IGestureRecognizer
{
public:
	virtual ~RightHandCloseToArmRecognizer() {}

	virtual Fubi::RecognitionResult::Result recognizeOn(FubiUser* user, Fubi::RecognitionCorrectionHint* correctionHint = 0x0);
	virtual IGestureRecognizer* clone() { return new RightHandCloseToArmRecognizer(*this); }
};