%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/downSample
%
%   Description:    
%		downSamples a signal from the input signal with the specified
%		integer factor
%
%   Parameters:
%		this	(signal)
%		factor  (double) every nth sample is retained, where n=factor
%
%   Returns:
%     this	(signal)
%
%	 $Revision $
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = downSample(this, factor)
DO_PLOT = false;

thisOrig = this;
thisSamples = get(this, 'samples');
thisSamples = thisSamples(1:factor:end);
this = set(this, 'samples', thisSamples);
this = set(this, 'Fs', getFs(this)/factor);
this = set(this, 'extent', length(thisSamples));

if DO_PLOT
    figure;
    plotParams.X.scale = 1; plotParams.X.translate = 0;
    plotParams.Y.scale = 1/150; plotParams.Y.translate = 0;
    plotParams.visible = 1;
    plotParams.textStr = '';
    DO_HOLD = true;
    
    plotParams.formatStr ='b-';
    plot(thisOrig, plotParams, DO_HOLD);
    
    plotParams.X.scale = 1/factor; plotParams.X.translate = factor - 1;
    plotParams.formatStr ='r-.';
    plot(this, plotParams, DO_HOLD);
end
return
