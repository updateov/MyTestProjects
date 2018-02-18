%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/filterByType
%
%   Description:    
%	   returns only the elements of this of the specified class
%
%	 Parameters:
%       this              (fhrPartSet)  
%       className         (cell)  the names of the subclass.  It the last
%                         name in this cell is 'exact', the class names
%                         should match exactly.  Otherwise any child class
%                         can match as well
%
%   Returns:
%       out              (fhrPartSet)  the filtered fhrParts
%       inIndices          (double)   the original indices of the filtered
%                                   elements
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [out, inIndices] = filterByType(this, classNames)

CHECK_FOR_FHR_PART2 = isa(getFhrPartParser(this), 'fhrPartParser2');  
if CHECK_FOR_FHR_PART2
    len = length(classNames);
    for i = 1:len
        classNames{len + i} = [classNames{i} '2'];
    end
end

if strcmp(classNames{end}, 'exact')
    exact = true;
    classNames(end) = [];
else
    exact = false;
end
indices = zeros(1, 10000);
nOutElements = 0;
PROLONGED_LEN = (2 * 60 * 4);
for i = 1:length(this.set)
    for j = 1:length(classNames)
        
        if (~exact & isa(this.set{i}, classNames{j})) | ...
           ( exact & strcmp(class(this.set{i}), classNames{j}))
                nOutElements = nOutElements + 1;
                indices(nOutElements) = i;
                break;
                
        elseif (strcmp(lower(classNames{j}), 'prolongeddecel'))
            if isa(this.set{i}, 'decel')
                if length(this.set{i}) >= PROLONGED_LEN
                    nOutElements = nOutElements + 1;
                    indices(nOutElements) = i;
                    break;
                end
            end
        end
    end
end
indices(nOutElements + 1:end) = [];
out = keepIndices(this, indices);
% out = fhrPartSet(this.set{indices});
if nargout == 2
    inIndices = indices;
end
return
