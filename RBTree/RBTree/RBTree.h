// RBRBTree.h

#pragma once

class RBTreeElement
{
public:
	int m_val;
};

class RBTree
{
public:
	RBTree();
	~RBTree();

private:
	//RBTree* m_pDummy;

public:
	RBTree* m_pParent;
	int m_color;
	RBTreeElement* m_pElement;
	RBTree* m_pLeft;
	RBTree* m_pRight;

public:
	void LeftRotate(RBTree** root, RBTree* vert);
	void RightRotate(RBTree** root, RBTree* vert);
	void RBTreeInsert(RBTree** root, RBTree* box);
	void RBInsert(RBTree** root, RBTree* box);
	void AddRBTree(RBTree** root, RBTreeElement* data);
	RBTree* RBDelete(RBTree** root, RBTree* vert);
	void RBDeleteFixup (RBTree** root, RBTree* vert);
	RBTree* Successor (RBTree* box);
	RBTree* RBTreeMinimum (RBTree* box);
	RBTree* SearchItem (RBTree* root, int val);
	void InitDummy ();
	RBTree* GetDummy ();


};

