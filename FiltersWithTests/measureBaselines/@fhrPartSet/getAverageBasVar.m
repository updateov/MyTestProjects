%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/getAverageBasVar
%
%   Description:    
%	   Returns the average variability weighted by baseline length of a set
%	   of baselines
%
%	 Parameters:
%      this              (fhrPartSet)  
%
%   Returns:
%       avgBasVar	          (double)
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function avgBasVar = getAverageBasVar(this)
sum = 0;
    if isa(this,'fhrPartSet') 
        baselines = getRawData(this);
        for index = 1:length(baselines)
            baseline = baselines{index};
            if isa(baseline, 'baselineWithVar')
                variability = getVar(baseline);
                basLength = length(getInterval(baseline));
                sum = sum + variability * basLength;
            else
                error('Argument is not a baseline with variability.');
            end
        end%for
        totalBasLength = calcTotalLength(this);
        avgBasVar = sum / totalBasLength;
    else
        error('Argument is not a baseline set with variability.');
    end
end%getAverageBasVar
