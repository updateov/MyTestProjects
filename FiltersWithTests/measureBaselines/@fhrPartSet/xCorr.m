%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%   
%   function @fhrPartSet/xCorr
%
%   Description:    
%	   finds the cross correlation between two fhrPartSets, both overall
%	   and by individual part in fhrPartSet1.
%
%	 Parameters:
%      fhrPartSet1       (fhrPartSet)  
%      fhrPartSet2       (fhrPartSet)  
%
%   Returns:
%      overall           (double) the overall measure 
%      individual        (double) a vector of measures for each element of
%                                 fhrPartSet1.  
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function [overall, individual] = xCorr(fhrPartSet1, fhrPartSet2, fhr)
individual = zeros(1, length(fhrPartSet1));
 
DO_RESAMPLE = false;
GAP_VALUE = 0;
sig2 = toUniformSignal(fhrPartSet2, GAP_VALUE, fhr);
% debug
% figure;
% hold on;
% debug
for i = 1:length(fhrPartSet1.set)
    currPart = fhrPartSet1.set{i};
    sig1 = toSignal(currPart, fhr);
%    sig1 = translate(sig1, -meanFhr);
    sig2Extract = toSignal(sig2, getX1(currPart):min(getX2(currPart), length(sig2)), DO_RESAMPLE);
    individual(i) = xCorr(sig1, sig2Extract);
end
sig1 = toUniformSignal(fhrPartSet1, GAP_VALUE, fhr);
overall = xCorr(sig1, sig2);
return 