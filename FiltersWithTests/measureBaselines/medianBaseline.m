function uMedian  = medianBaseline(u, config)

nSamples = length(u);

uSignal = uniformSignal(u);
uMedian = getSamples(median(uSignal, config.MEDIAN_WIDTH, config.MEDIAN_EXTRAP_METHOD));
uMedian(isnan(u)) = NaN;
% 
% uMedian = nan(1, nSamples);
% 
% x1 = getX1(nonNanIntervals);
% x2 = getX2(nonNanIntervals);
% for jInterval = 1:length(x1)
%     uSignal = uniformSignal(u(x1(jInterval):x2(jInterval)));
%     currSamples = getSamples(median(uSignal, config.MEDIAN_WIDTH));
%     uMedian(x1(jInterval):x2(jInterval)) = currSamples;
% end


% nSamples = length(u);
% t = 0:seconds(config.Ts):seconds((nSamples-1)*config.Ts);
% config.COLOR = 'm';

% aFigManager = getFigureByName(aFigManager, 'baseline filtering');
% subplot(2,1,1);
% plot(t, uMedian, 'Color', config.COLOR, 'LineWidth', 2, 'DurationTickFormat','hh:mm:ss');

% fhr.medianRes = fhr.repair-fhr.median;
% subplot(2,1,2);
% plot(t, fhr.medianRes, 'Color', 'k', 'LineWidth', 1, 'DurationTickFormat','hh:mm:ss');
% grid off;
% grid on;
% grid minor;
% hold on;
% 
% hAxis(2) = gca;
% linkaxes(hAxis, 'x');
% 
% zoom xon;
% 

% aFigManager = getFigureByName(aFigManager, 'emd residual (db)');
% hold on;
% rParabEmd = rParabEmd__L (fhr.medianRes, 40, 40, 1);
% 
% aFigManager = getFigureByName(aFigManager, 'median-removed emd');
% nComponents = size(rParabEmd, 2);
% for iComponent = 1:nComponents
%     subplot(nComponents,1,iComponent);
%     plot(t, rParabEmd(:, iComponent), 'Color', config.COLOR, 'LineWidth', 2, 'DurationTickFormat','hh:mm:ss');
% end
% 
% fhr.medianResEmd = rParabEmd(:, end);
% 
% aFigManager = getFigureByName(aFigManager, 'median-removed FHR');
% plot(t, fhr.medianResEmd, 'Color', 'k', 'LineWidth', 1, 'DurationTickFormat','hh:mm:ss');

% [fhr.klBaseline aFigManager] = klBaseline(fhr.medianRes, config, aFigManager);
% aFigManager = getFigureByName(aFigManager, 'median-removed FHR');
% subplot(2,1,2);
% plot(t, fhr.klBaseline, 'Color', 'r', 'LineWidth', 1, 'DurationTickFormat','hh:mm:ss');
% 
% subplot(2,1,1);
% plot(t, fhr.median+fhr.klBaseline, 'Color', 'r', 'LineWidth', 1, 'DurationTickFormat','hh:mm:ss');

return                                 