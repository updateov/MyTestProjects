%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/filterByFreqBand
%
%   Description:    
%	   filters out any bumps in fhrPartSet that are not from the specified
%	   frequency band
%
%	 Parameters:
%       this              (fhrPartSet)  
%       freqBands         (array of double)
%
%   Returns:
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = filterByFreqBand(this, freqBands)
indices = zeros(1, 10000);
nOutElements = 0;
for i = 1:length(this.set)
    keepPart = 1;
    if isa((this.set{i}), 'bump')
        currBand = getFreqBand(this.set{i});
        if ((currBand > 0) && (sum(currBand == freqBands) == 0))
            keepPart = 0;
        end
    end
    if keepPart
        nOutElements = nOutElements + 1;
        indices(nOutElements) = i;
    end
end
indices(nOutElements + 1:end) = [];
this = fhrPartSet(this.set{indices});
if nargout == 2
    inIndices = indices;
end
return

