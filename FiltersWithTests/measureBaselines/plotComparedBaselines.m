function plotComparedBaselines(baselines, classifyResults, config)

nParts = length(baselines);
%samp2Day=config.Ts/3600/24;
%samp2Day=config.Ts;
unitsPerSample = config.Ts;

alphaVec = [0.5 0.5 0.5 0.5];
colorStr = {'c', 'r', 'b', 'm'};   
widthVal = [2, 2, 2, 2];
classSuccessStr = {'TN', 'FN', 'FP', 'TP'};
theSet = getRawData(baselines);

for iCol = 1:length(colorStr)
    hBase(iCol) = line([0 0],[0 0], 'LineWidth', widthVal(iCol), 'LineStyle', '-', 'Color', colorStr{iCol});
end
legend(hBase, classSuccessStr, 'Location', 'Best');

baseIntervals = toIntervals(baselines);
x1 = getX1(baseIntervals);
x2 = getX2(baseIntervals);
y1 = nan(1, nParts);
y2 = nan(1, nParts);
for iPart = 1:nParts
    y1(iPart) = getY1(theSet{iPart});
    y2(iPart) = getY2(theSet{iPart});
end

for j = 1:length(colorStr)
    currI = find(classifyResults==j);
    currNParts = length(currI);
    currX = [x1(currI); x2(currI)];
    currX = reshape(currX, 1, numel(currX));
    currY = [y1(currI); y2(currI)];
    currY = reshape(currY, 1, numel(currY));
    
    currDrawI = 1:3*currNParts;
    currNoDrawI = find(mod(currDrawI, 3)==0);
    currDrawI(currNoDrawI) = [];
    
    currXn = ones(1, 3*currNParts);
    currXn(currDrawI) = currX;
    currYn = nan(1, 3*currNParts);
    currYn(currDrawI) = currY;
    patchline(currXn*unitsPerSample, ...
              currYn, ...
              'edgecolor', colorStr{j}, ...
              'linewidth',2,'edgealpha',alphaVec(j), 'Marker', 'o', 'MarkerSize', 3);
end

grid off;
grid on;
grid minor;

drawnow;

return