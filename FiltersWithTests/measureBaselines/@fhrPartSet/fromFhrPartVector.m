%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/fromFhrPartVector
%
%   Description:    
%	   sets the state of this using the contents of the specified 
%      fhr_part_vector
%
%	 Parameters:
%      this              (fhrPartSet)  
%      fhrPartVector     (fhr_part_vector structure array) 
%
%   Returns:
%       this	          (fhrPartSet)
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = fromFhrPartVector(this, fhrPartVector)
warning off MATLAB:polyfit:RepeatedPointsOrRescale;
if length(fhrPartVector) > 0
    isFhrPart2 = checkFhrPart2(fhrPartVector(1));

    if isFhrPart2
        this = setFhrPartParser(this, fhrPartParser2);
    else
        this = setFhrPartParser(this, fhrPartParser);
    end

    for i=1:length(fhrPartVector)
    
        fhrPart = parse(getFhrPartParser(this), fhrPartVector(i));
        this = add(this, fhrPart);
    end
else
    this = fhrPartSet;
end
return

function b = checkFhrPart2(f)

    b = (isfield(f, 'percRepair'));
    
return;
