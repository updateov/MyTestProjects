%%%%%
%
%   merge parts that overlap, regardless of type
%
%%%%%%%%%%%%%%%%%%%%%%%%%

function s = mergeOverlapParts(s)

    s = sort(s);
    len = length(s);
    if len < 2
        return;
    end
   
    keepInd = zeros(1, length(s));
    n = 0;
    
    for i = 1:len-1
        if (intersect(s.set{i}, s.set{i+1}))
            s.set{i+1} = setX1(s.set{i+1}, getX1(s.set{i}));
        else
            n = n + 1;
            keepInd(n) = i;
        end
    end
    
    n = n + 1;
    keepInd(n) = len;
    
    
    
    keepInd = keepInd(1:n);
    s = keepIndices(s, keepInd);
    
return;



        