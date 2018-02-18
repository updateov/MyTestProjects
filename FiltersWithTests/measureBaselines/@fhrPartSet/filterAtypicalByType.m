%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/filterAtypicalByType
%
%   Description:    
%	   returns only the elements of the fhrPartSet which are atypical
%	   decels of the specified type(s).  If more than one type is specified
%	   then will return any decel with any one of the specified types.
%
%	 Parameters:
%       this              (fhrPartSet)  
%       atypTypes         (cell) the names of atypical features.  Can be
%                           strings ('biphasic', 'lossRise',
%                           'prolongedSecRise', 'slowReturn', 'lowerBaseline',
%                           'lossVar', 'sixties') or partChars ('BI', 'LR', 'P2',
%                           'SR', 'LB', 'LV' and '60')
%
%   Returns:
%       out              (fhrPartSet)  the filtered fhrParts
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [out] = filterAtypicalByType(this, atypTypes)

this = filterByType(this, {'atypicalVariableDecel'});

indices = zeros(1, 10000);
nOutElements = 0;

for i = 1:length(this.set)
    for j = 1:length(atypTypes)
        if hasFeatures(this.set{i}, atypTypes(j))
                nOutElements = nOutElements + 1;
                indices(nOutElements) = i;
                break;
        end
    end
end
indices(nOutElements + 1:end) = [];
out = fhrPartSet(this.set{indices});

return
