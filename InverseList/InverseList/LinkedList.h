#pragma once
class LinkedList
{
public:
	LinkedList(int val);
	LinkedList(int val, LinkedList* pNext);
	virtual ~LinkedList();

private:
	LinkedList* m_pNext;
	int m_val;

public:
	LinkedList* GetNext();
	void SetNext(LinkedList* pNode);
	int GetVal();
	void AddHead(LinkedList** pHead, LinkedList* pNode);

};

class Empty
{
};