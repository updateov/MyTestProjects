function inputNames = createHash(inputNames)
for i=1:length(inputNames.str)
    inputNames.hash.(inputNames.str{i}) = i;
end
return