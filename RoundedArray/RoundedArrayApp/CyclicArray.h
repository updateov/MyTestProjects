#pragma once

using namespace std;

#define ARRAY_SIZE 16
#define ARRAY_MAX_ELEM_SIZE 15
#define NaN -255

//#define NaN numeric_limits<double>::quiet_NaN()

class CyclicArray
{
public:
    CyclicArray();
    virtual ~CyclicArray();

private:
    double m_array[ARRAY_SIZE];

    int m_currentHead;
    int m_currentTail;
    int m_size;

public:
    double at(int position);
    int size() { return m_size; }
    int mod(int position);

    double back();
    double front();

    void clear();

    void insert_range(int position, double value);
    double remove_head();
    void push_back(const double value);

    double operator[](int position) { return m_array[mod(m_currentHead + position)]; }
};

