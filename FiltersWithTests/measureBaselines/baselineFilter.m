function uLp = baselineFilter(u, config)
addPathToBase('data\common\filter');
addPathToBase('src\common\filter');


if isfield(config, 'HP_FILT_NAME')
    HP_FILT_NAME = config.HP_FILT_NAME;
else 
    %HP_FILT_NAME = 'fir_hp90s';
    HP_FILT_NAME = 'fir_hp200s';
    %HP_FILT_NAME = 'fir_hp500s';
    %HP_FILT_NAME = 'fir_bp660_400s';
end

% remove low-pass: detrend
load(HP_FILT_NAME);
eval(['hpFilt = ' HP_FILT_NAME ';']);

uHp = filt_del(u, hpFilt);

if config.IS_LP
    uLp = uHp;
else
    uLp = u - uHp;
end    

% aFigManager = getFigureByName(aFigManager, 'baseline filtering');
% 
% nSamples = length(fhr.raw);
% t = 0:seconds(config.Ts):seconds((nSamples-1)*config.Ts);
% 
% subplot(2,1,1);
% plot(t, u, 'k', 'LineWidth', 1, 'DurationTickFormat','hh:mm:ss');
% hAxis(1) = gca;
% hold on;
% plot(t, uLp, 'Color', config.COLOR, 'LineWidth', 2, 'DurationTickFormat','hh:mm:ss');
% ax = axis;
% axis([ax(1:2) 30 240]);
% grid off;
% grid on;
% grid minor;
% 
% subplot(2,1,2);
% plot(t, up.raw, 'g', 'DurationTickFormat','hh:mm:ss', 'LineWidth', 1);
% hAxis(2) = gca;
% ax = axis;
% axis([ax(1:2) 0 100]);
% grid off;
% grid on;
% grid minor;
% 
% linkaxes(hAxis, 'x');
% 
% zoom xon;
% 

return
