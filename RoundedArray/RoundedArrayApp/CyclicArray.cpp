#include "stdafx.h"
#include "CyclicArray.h"

using namespace std;

CyclicArray::CyclicArray()
{
    clear();
}

CyclicArray::~CyclicArray()
{
}

double CyclicArray::at(int position)
{
    return m_array[mod(position)];
}

int CyclicArray::mod(int position)
{
    int toRet = position % ARRAY_SIZE;
    if (toRet < 0)
    {
        toRet += ARRAY_SIZE;
        toRet %= ARRAY_SIZE;
    }

    return toRet;
}

double CyclicArray::back()
{
    return m_currentTail > -1 ? m_array[m_currentTail] : NaN;
}

double CyclicArray::front()
{
    return m_array[m_currentHead];
}

void CyclicArray::clear()
{
    for (int i = 0; i < ARRAY_SIZE; i++)
    {
        m_array[i] = NaN;
    }

    m_currentHead = 0;
    m_currentTail = 0;
    m_size = 0;
}

double CyclicArray::remove_head()
{
    if (m_size == 0)
        return NaN;

    double toRet = m_array[m_currentHead];
    m_array[m_currentHead++] = NaN;
    m_currentHead = mod(m_currentHead);
    --m_size;
    return toRet;
}

void CyclicArray::push_back(const double value)
{
    m_currentTail = mod(m_currentTail);

    m_array[m_currentTail++] = value;
    ++m_size;
    if (m_size > ARRAY_MAX_ELEM_SIZE)
    {
        m_array[m_currentHead++] = NaN;
        m_size = ARRAY_MAX_ELEM_SIZE;
    }
}