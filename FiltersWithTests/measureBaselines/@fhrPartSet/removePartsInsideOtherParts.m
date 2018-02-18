function s = removePartsInsideOtherParts(s)

    noRem = false;
    while (~noRem)
        [s, noRem] = removeLoop(s);
    end
    
return;

function [s, noRem] = removeLoop(s)

    len = length(s);
    if len < 2
        noRem = true;
        return;
    end
   
    keepInd = zeros(1, length(s));
    n = 0;
    
    if ~inside(s.set{1}, s.set{2})
        n = n + 1;
        keepInd(n) = 1;
    end
        
    for i = 2:len-1
        s.set{i}
        if (~inside(s.set{i}, s.set{i-1})) && (~inside(s.set{i}, s.set{i+1})) 
            n = n + 1;
            keepInd(n) = i;
        end
    end
    
    if ~inside(s.set{len}, s.set{len - 1})
        n = n + 1;
        keepInd(n) = len;
    end
    
    keepInd = keepInd(1:n);
    s = keepIndices(s, keepInd);
    
    noRem = (n == len);
    
return;

function b = inside(s1, s2)

    b = false;
    if (getX1(s1) >= getX1(s2)) && (getX2(s1) <= getX2(s2))
        b = true;
    end
    
return;


        