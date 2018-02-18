function plotEpochs(totalDuration, epochLen, tickLen, id, outDir, config)

id = strrep(id, '\', '_');
filename = sprintf('%s-overall.pdf', id);
printPdf(gcf, filename);

ax = axis;
currOffset = 0;

while currOffset < totalDuration - epochLen
    axis([currOffset + [0 epochLen] ax(3:4)]);
    set(gca, 'XTick', currOffset:tickLen:currOffset+epochLen);
    filename = sprintf('%s-%05.0fm.pdf', id, currOffset*24*60);
    printPdf(gcf, filename);
    
    currOffset = currOffset + epochLen;
end
outDirEpoch = [outDir '\Epochs'];
mkdir(outDirEpoch);
movefile(sprintf('%s*.pdf', id), outDirEpoch);

return