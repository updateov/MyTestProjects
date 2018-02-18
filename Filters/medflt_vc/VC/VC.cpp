//
//
//  VC.cpp - sample program to demonstrate how to use fast median filter class.
//
//  Project: Fast median filter
//  Author: S.Zabinskis
//
//  May, 2008
//
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//
//

#include "stdafx.h"



int _tmain(int argc, _TCHAR* argv[])
{
	TMedianFilter1D<int> flt;
	std::vector<int> v1;

	v1.push_back(1);
	v1.push_back(2);
	v1.push_back(0);
	v1.push_back(-1);
	v1.push_back(1);
	v1.push_back(5);
	v1.push_back(3);
	v1.push_back(-2);

	flt.SetWindowSize(3	);

	flt.Execute(v1, true);

	for(int j=0; j<flt.Count(); j++)
	{
		std::cout << flt[j] << std::endl;
	}
	return 0;
}

