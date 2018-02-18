// CyclicArrayApp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include "CyclicArray.h"

int _tmain(int argc, _TCHAR* argv[])
{
    int modBase = 7;
    int mod = -9 % modBase;
    int toMod = mod + modBase;
    int newMod = toMod % modBase;

    CyclicArray ca;
    for (int i = 0; i < 27; i++)
    {
        ca.push_back(i * 2);
        std::cout << "Value at " << ca.mod(i) << " = " << ca[i] << " " << "size = " << ca.size() << "\n";
    }

    for (int i = 0; i < ARRAY_SIZE; i++)
    {
        std::cout << "Value at " << ca.mod(i) << " = " << ca[i] << " " << "size = " << ca.size() << "\n";
        //std::cout << ca.remove_head() << " " << "size = " << ca.size() << "\n";
    }

    for (int i = 0; i < 20; i++)
    {
        double val = ca.remove_head();
        std::cout << "Removed head value = " << val << ", size = " << ca.size() << "\n";
    }

    for (int i = 0; i < ARRAY_SIZE; i++)
    {
        std::cout << "Value at " << ca.mod(i) << " = " << ca[i] << " " << "size = " << ca.size() << "\n";
    }

    char str;
    std::cin >> str;
	return 0;
}

