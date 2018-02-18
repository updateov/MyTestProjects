%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/toFhrPartVector
%
%   Description:    
%	   converts this to an fhr_part_vector
%
%	 Parameters:
%      this              (fhrPartSet)  
%
%   Returns:
%      fhrPartVector     (fhr_part_vector structure array) 
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function fhrPartVector = toFhrPartVector(this)
for i = 1:length(this.set)
%    fhrPartVector(i) = toFhrPartVectorElement(this.set{i});
    fhrPartVector(i) = write(this.fhrPartParser, this.set{i});
end
if isempty(this.set)
    fhrPartVector = [];
end
return
