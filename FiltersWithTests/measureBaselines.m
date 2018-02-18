function [aClassExamples out] = measureBaselines(id, fhr, baselines, ...
                                           trueIndices, predictedIndices, ...
                                           outDir, config)

addPathToBase('src\common\util\api\nlid\nlid_tools');
addPathToBase('src\common\util\api\nlid\utility_tools');
addPathToBase('src\common\learning');

addPathToBase('src-test\analysis\sysID\subspace');
addPathToBase('src-test\patterns\2ndPass');

close all;
aFigManager = figManager;
aFigManager = setDocking(aFigManager, config.DO_FIGURE_MGR_DOCKING);

nSamples = length(fhr.raw);
t = 0:seconds(config.Ts):seconds((nSamples-1)*config.Ts);
%t = 0:(nSamples-1);
%samp2Day=config.Ts/3600/24;
%samp2Day=1;
%samp2Day=config.Ts;
unitsPerSample=config.Ts;

%totalDuration = days(nSamples*samp2Day);
totalDuration = nSamples*unitsPerSample;

nParts = length(baselines);

TNi = trueIndices==0 & predictedIndices==0;
FPi = trueIndices==0 & predictedIndices==1;
FNi = trueIndices==1 & predictedIndices==0;
TPi = trueIndices==1 & predictedIndices==1;

classifyResults = nan(1, nParts);
classifyResults(TNi) = 1; %TN
classifyResults(FNi) = 2; %FN
classifyResults(FPi) = 3; %FP
classifyResults(TPi) = 4; %TP

N = length(find(trueIndices==0));
P = length(find(trueIndices==1));
TN = length(find(TNi));
TP = length(find(TPi));
out.theConfusion = confusion2(N, P, TN, TP);
display(out.theConfusion);

if config.SKIP_MEASURES
    aClassExamples = classExamples;
    return;
end

[fhr.repair repairParts] = fhr_repair_offline_new(fhr.raw);

repairIntervals = toIntervals(repairParts);

isNaNSignal = toLogical(repairIntervals, length(fhr.repair));
fhr.repair2NaN = fhr.repair;
fhr.repair2NaN(isNaNSignal==1) = NaN;

aFigManager = getFigureByName(aFigManager, 'baseline Classification');
subplot(3,3,1:3);
h(1)=plot(t, fhr.repair, 'Color', 'k', 'LineWidth', 1, 'DurationTickFormat', config.durationTickFormatStr);
ylabel('Repair');
hold on;
ax = axis;
axis([ax(1:2) 30 240]);
grid off;
grid on;
grid minor;
hAxis(1) = gca;

config.MEDIAN_WIDTH = 4800;
config.MEDIAN_EXTRAP_METHOD='mirror';

fhr.median20 = medianBaseline(fhr.repair2NaN, config);
h(2)=plot(t, fhr.median20, 'Color', 'g', 'LineWidth', 1, 'DurationTickFormat', config.durationTickFormatStr);

drawnow;

plotComparedBaselines(baselines, classifyResults, config)

%aFigManager = getFigureByName(aFigManager, 'median-removed FHR');
%subplot(2,2,1);
%h(1)=plot(t, fhr.repair, 'Color', 'k', 'LineWidth', 1, 'DurationTickFormat', config.durationTickFormatStr);
%h(1)=plot(t, fhr.repair, 'Color', 'k', 'LineWidth', 1);
%hold on;
%drawnow;


%h(2)=plot(t, fhr.median20, 'Color', 'b', 'LineWidth', 2);
% ax = axis;
% axis([ax(1:2) 30 240]);
% grid off;
% grid on;
% grid minor;
% hAxis(1) = gca;
% ylabel('FHR (bpm)');
% legend(h, {'Repair', 'Median'});

subplot(3,3,4:6);
fhr.median20Res = fhr.repair-fhr.median20;
hRes(1) = plot(t, fhr.median20Res, 'Color', 'b', 'LineWidth', 1, 'DurationTickFormat', config.durationTickFormatStr);
legendStr{1} = 'median20Res';
%plot(t, fhr.median20Res, 'Color', 'b', 'LineWidth', 1);
grid off;
grid on;
grid minor;
hold on;
hAxis(2) = gca;
ylabel('Residue');

% subplot(2,2,3);
% ax = gca;
% UserData.series=fhr.median20Res;
% UserData.x1=1;
% UserData.x2=nSamples;
% UserData.x1Orig = 1;
% UserData.x2Orig = nSamples;
% set(ax, 'UserData', UserData);
% ax.NextPlot = 'add';
% refreshHist(ax);

%subplot(2,2,2);
fhr.median20ResHalfWavePos = fhr.median20Res;
fhr.median20ResHalfWavePos(fhr.median20ResHalfWavePos < 0) = NaN;
fhr.median20ResHalfWavePosInterp = interpNan(fhr.median20ResHalfWavePos, 'linear', true);
%config.HP_FILT_NAME = 'fir_hp200s';
config.HP_FILT_NAME = 'fir_hp25s';
config.IS_LP = false;
fhr.upperEnvelope = baselineFilter(fhr.median20ResHalfWavePosInterp, config);
hRes(2) = plot(t, fhr.upperEnvelope, 'g', 'DurationTickFormat', config.durationTickFormatStr);
legendStr{2} = 'upEnv';

fhr.median20ResHalfWaveNeg = fhr.median20Res;
fhr.median20ResHalfWaveNeg(fhr.median20ResHalfWaveNeg >= 0) = NaN;
fhr.median20ResHalfWaveNegInterp = interpNan(fhr.median20ResHalfWaveNeg, 'linear', true);
%config.HP_FILT_NAME = 'fir_hp200s';
config.HP_FILT_NAME = 'fir_hp25s';
config.IS_LP = false;
fhr.lowerEnvelope = baselineFilter(fhr.median20ResHalfWaveNegInterp, config);
hRes(3) = plot(t, fhr.lowerEnvelope, 'r', 'DurationTickFormat', config.durationTickFormatStr);
legendStr{3} = 'lowEnv';

legend(hRes, legendStr);

% subplot(2,2,1);
fhr.upper = fhr.median20 + fhr.upperEnvelope;
%plot(t, fhr.upper, 'g', 'LineWidth', 2);
fhr.lower = fhr.median20 + fhr.lowerEnvelope;
%plot(t, fhr.lower, 'r', 'LineWidth', 2);

%subplot(2,2,2);
fhr.median20ResInterp = interpNan(fhr.median20Res, 'linear', true);
%config.HP_FILT_NAME = 'fir_hp200s';
config.HP_FILT_NAME = 'fir_hp25s';

config.IS_LP = false;
fhr.median20ResLp = baselineFilter(fhr.median20ResInterp, config);
%plot(t, fhr.median20ResLp, 'k');
% 
% subplot(2,2,1);
% fhr.upper = fhr.median20 + fhr.median20ResLp;
% plot(t, fhr.upper, 'g', 'LineWidth', 2);

%zoom xon;
zoom;

% subplot(2,2,2);
% y = hilbert(fhr.median20ResInterp);
% plot(t, abs(y), 'g');
% plot(t, -abs(y), 'r');

% create the residual spline signal
% tDecimate = 1:length(t);
% tDecimate = tDecimate(1):config.RESIDUAL_DECIMATE_FACTOR:tDecimate(end);
% 
% isInValidI = find(isNaNSignal);
% isValidI = find(~isNaNSignal);
% tDecimateValid = setdiff(tDecimate, isInValidI);
% 
% nDecimatedSamples = length(tDecimate);
% fhrMedian20ResSpline = nan(1, nDecimatedSamples);
% fhrMedian20ResSpline(floor(tDecimateValid/config.RESIDUAL_DECIMATE_FACTOR)) = ...
%     spline(isValidI,fhr.median20Res(isValidI),tDecimateValid);
% 
% fhrMedian20ResSignal = uniformSignal('res', config.Fs/config.RESIDUAL_DECIMATE_FACTOR, ...
%                    length(fhrMedian20ResSpline), fhrMedian20ResSpline);
%                
% subplot(2,2,2);
% plot(seconds(tDecimate), fhrMedian20ResSpline, 'k');
      

%get the extrema of residual
% config.COLOR = 'b';
% config.INTERVAL_LENGTH = 4800;
% config.OVERLAP_RATIO = 0.5;
% config.N_PLOTS = 2;
% config.N_PLOTS = 4;
% config.CHAN_NAMES{1, 1} = 'UP';
% config.CHAN_NAMES{2, 1} = 'FHR';
% config.CHAN_UNITS{1} = {''};
% config.CHAN_UNITS{2} = {'bpm'};
% config.CHAN_COMMENTS{1} = '';
% config.CHAN_COMMENTS{2} = '';
% config.EXTREMA_CHEBY_DO_FILTER_END_EXTREMA = true;
% 
% 
% %resExtrema = extremaCheby(fhrMedian20ResSignal, config);
% resExtrema = extremaCheby(uniformSignal(fhr.upperEnvelope), config);
% aFigManager = getFigureByName(aFigManager, 'median-removed FHR');
% subplot(2,2,2);
% maxima = getMaxima(resExtrema);
% %plot(tDecimate(getIndices(maxima)), getSamples(maxima), 'go');
% plot(t(getIndices(maxima)), getSamples(maxima), 'go');
% minima = getMinima(resExtrema);

% plot(tDecimate(getIndices(minima)), getSamples(minima), 'ro');

% plot(tDecimate, spline(tDecimate(getIndices(maxima)), getSamples(maxima), tDecimate), 'g-');
% plot(tDecimate, spline(tDecimate(getIndices(minima)), getSamples(minima), tDecimate), 'r-');


subplot(3,3,7:9);
config.SKEWNESS_WIDTH = 4800;
fhr.skewness20 = skewnessBaseline(fhr.median20Res, config);
%fhr.skewness20 = 1:nSamples;
h1(1)=plot(t, fhr.skewness20, 'Color', 'b', 'LineWidth', 2, 'DurationTickFormat', config.durationTickFormatStr);
legendSkew{1} = 'skew20';
hold on;
drawnow;


%config.SKEWNESS_WIDTH = 480;
config.SKEWNESS_WIDTH = 960;
fhr.skewness2 = skewnessBaseline(fhr.median20Res, config);
%fhr.skewness2 = skewnessBaseline(fhr.median20ResLp, config);
h1(2)=plot(t, fhr.skewness2, 'Color', 'r', 'LineWidth', 2, 'DurationTickFormat', config.durationTickFormatStr);
legendSkew{2} = 'skew2';

ax = axis;
axis([ax(1:2) -5 5]);
hAxis(3) = gca;
grid on;
grid minor;
ylabel('Skewness');
legend(h1, legendSkew);

linkaxes(hAxis, 'x');
%zoom xon;
% zoom;
% subplot(2,2,1);
% hZoom = zoom;
% hZoom.ActionPostCallback=@zoomCallback;

% epochLen = 30*60*config.Fs*samp2Day;
% tickLen = 10*60*config.Fs*samp2Day;
epochLen = 30*60;
tickLen = 10*60;
if config.DO_PLOT_EPOCHS
    plotEpochs(totalDuration, epochLen, tickLen, id, outDir, config);
end    

inputNames.str = {'amplitude', 'variability', 'skewness20', 'skewness2', 'upperEnvelope',...
                  'lowerEnvelope'}; % 'prevDeltaAmp', 'nextDeltaAmp'};
inputNames = createHash(inputNames);
outputNames = {'isBaseline'};
nInputs = length(inputNames.str);
exampleData = [];
idEx = cell(1, nParts);
bl.amp = nan(1, nParts);
bl.var = nan(1, nParts);
bl.skew20 = nan(1, nParts);
bl.skew2 = nan(1, nParts);
bl.upperEnv = nan(1, nParts);
bl.lowerEnv = nan(1, nParts);
%bl.ampPrevDelta = nan(1, nParts);
%bl.ampNextDelta = nan(1, nParts);

classNames = {'falseBL', 'trueBL'};
classTargetMap = [0 1];
theSet = getRawData(baselines);

for iCand = 1:nParts   
    x1 = getX1(theSet{iCand});
    x2 = getX2(theSet{iCand});
    y1 = getY1(theSet{iCand});
    y2 = getY2(theSet{iCand});
    
    blMean = (y1 + y2)/2;
    medMean = nanmean(fhr.median20(x1:x2));
    
    bl.amp(iCand) = blMean - medMean;
    
    bl.var(iCand) = nanstd(fhr.median20Res(x1:x2));
    
    bl.skew20(iCand) = nanmean(fhr.skewness20(x1:x2));
    bl.skew2(iCand) = nanmean(fhr.skewness2(x1:x2));

    bl.upperEnv(iCand) = nanmean(fhr.upperEnvelope(x1:x2));
    bl.lowerEnv(iCand) = nanmean(fhr.lowerEnvelope(x1:x2));

%     if iCand > 1
%         bl.ampPrevDelta(iCand) = bl.amp(iCand)-bl.amp(iCand-1);
%     else        
%         bl.ampPrevDelta(iCand) = bl.amp(iCand)-bl.amp(iCand+1);
%     end
% 
%     if iCand < nParts
%         bl.ampNextDelta(iCand) = bl.amp(iCand+1)-bl.amp(iCand);
%     else        
%         bl.ampNextDelta(iCand) = bl.ampPrevDelta(iCand);
%     end

end

%save('tmp');

inputCnt = 0;

inputCnt = inputCnt+1;
exampleData(inputNames.hash.(inputNames.str{inputCnt}), :) = bl.amp;
inputCnt = inputCnt+1;
exampleData(inputNames.hash.(inputNames.str{inputCnt}), :) = bl.var;
inputCnt = inputCnt+1;
exampleData(inputNames.hash.(inputNames.str{inputCnt}), :) = bl.skew20;
inputCnt = inputCnt+1;
exampleData(inputNames.hash.(inputNames.str{inputCnt}), :) = bl.skew2;
inputCnt = inputCnt+1;
exampleData(inputNames.hash.(inputNames.str{inputCnt}), :) = bl.upperEnv;
inputCnt = inputCnt+1;
exampleData(inputNames.hash.(inputNames.str{inputCnt}), :) = bl.lowerEnv;
% inputCnt = inputCnt+1;
% exampleData(inputNames.hash.(inputNames.str{inputCnt}), :) = bl.ampPrevDelta;
% inputCnt = inputCnt+1;
% exampleData(inputNames.hash.(inputNames.str{inputCnt}), :) = bl.ampNextDelta;

% aClassExamples = classExamples({exampleData}, {predictedIndices}, {id}, ...
%                                inputNames.str, outputNames);
aClassExamples = classExamples({exampleData}, {trueIndices}, {id}, ...
                               inputNames.str, outputNames);
                           


aFigManager = getFigureByName(aFigManager, 'Feature histograms');

config.N_BINS = 25;
config.lineStyle = '-';
config.lineWidth = 1;
config.inputNamesDisplayed = inputNames.str;
config.doLog = logical(zeros(1, nInputs));
config.xLimits = cell(1, nInputs);

config.color = 'c';
falseEx = filterByTarget(aClassExamples, 0);
plotHistograms(falseEx, [], gcf, config);

config.color = 'm';
trueEx = filterByTarget(aClassExamples, 1);
plotHistograms(trueEx, [], gcf, config);

% aFigManager = getFigureByName(aFigManager, '2-D features');
% n2dPlots = nchoosek(nInputs, 2);
% nRows = ceil(n2dPlots^0.5);
% nColumns = ceil(n2dPlots/nRows);
% iPlot = 0;
% for iInput = 1:nInputs
%     for jInput = iInput+1:nInputs
%         iPlot = iPlot + 1;
%         subplot(nRows, nColumns, iPlot);
%         config.color = 'c';
%         config.markerStr = 'o';
%         config.markerSize = 4;
%         plot2d(falseEx, inputNames.str([iInput, jInput]), config);
%         config.color = 'm';
%         plot2d(trueEx, inputNames.str([iInput, jInput]), config);
%     end
% end


% blTable = table(bl.amp', bl.var', bl.skew', bl.ampSkew', bl.ampPrevDelta', bl.ampNextDelta', targetIndices', 'VariableNames', [inputNames.str 'className']);
% save('blTable.mat', 'blTable');



display(aClassExamples, true, 1);

if exist(outDir, 'dir') ~= 7 mkdir(outDir); end
[path fname ext] = fileparts(outDir);
%mkdir(['.\tmp\' fname]);
mkdir(['.\tmp\']);
printPdf(aFigManager, '.\tmp', [id '_']);
movefile(['.\tmp\*.pdf'], outDir,'f');

aFigManager = closeAll(aFigManager);


return

function zoomCallback(obj,event_obj)
newLim = event_obj.Axes.XLim;
newLim = floor(newLim * 24 * 3600 * 4);

subplot(2,2,3);
ax = gca;
ax.UserData.x1=max(newLim(1), ax.UserData.x1Orig);
ax.UserData.x2=min(newLim(2), ax.UserData.x2Orig);

refreshHist(ax);
return



function refreshHist(ax)
x1 = ax.UserData.x1;
x2 = ax.UserData.x2;
cla;
series = ax.UserData.series(x1:x2);
hist(series, 50);
ax = axis;
xlabel('Residue');
drawnow;

text('string', sprintf('Skewness=%5.2f', skewness(series)), 'interpreter', 'latex', 'pos', [(ax(2)-ax(1))*0.7+ax(1), (ax(4)-ax(3))*0.8+ax(3)]);
text('string', sprintf('$\\sigma=%5.2f$', nanstd(series)), 'interpreter', 'latex', 'pos', [(ax(2)-ax(1))*0.7+ax(1), (ax(4)-ax(3))*0.75+ax(3)]);
text('string', sprintf('$\\mu=%5.2f$', nanmean(series)), 'interpreter', 'latex', 'pos', [(ax(2)-ax(1))*0.7+ax(1), (ax(4)-ax(3))*0.7+ax(3)]);
drawnow;
return;

