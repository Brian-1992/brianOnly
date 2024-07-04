﻿Ext.define('WEBAPP.model.AB0039', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'ORDERCODE', type: 'string' }, 
        { name: 'INSUORDERCODE', type: 'string' }, 
        { name: 'ORDERENGNAME', type: 'string' }, 
        { name: 'SCIENTIFICNAME', type: 'string' }, 
        { name: 'ORDERHOSPNAME', type: 'string' }, 
        { name: 'ORDERCHINNAME', type: 'string' }, 
        { name: 'ORDEREASYNAME', type: 'string' }, 
        { name: 'COMMITTEECODE', type: 'string' }, 
        { name: 'DRUGCLASSIFY', type: 'string' }, 
        { name: 'ORDERUSETYPE', type: 'string' }, 
        { name: 'ORDERCONDCODE', type: 'string' }, 
        { name: 'DOHLICENSENO', type: 'string' }, 
        { name: 'MULTIPRESCRIPTIONCODE', type: 'string' }, 
        { name: 'DRUGELEMCODE1', type: 'string' }, 
        { name: 'COMPONENTNUNIT', type: 'string' }, 
        { name: 'DRUGELEMCODE2', type: 'string' }, 
        { name: 'COMPONENTNUNIT2', type: 'string' }, 
        { name: 'DRUGELEMCODE3', type: 'string' }, 
        { name: 'COMPONENTNUNIT3', type: 'string' }, 
        { name: 'DRUGELEMCODE4', type: 'string' }, 
        { name: 'COMPONENTNUNIT4', type: 'string' }, 
        { name: 'ORDERUNIT', type: 'string' }, 
        { name: 'ORDERCHINUNIT', type: 'string' }, 
        { name: 'SUPPLYNO', type: 'string' }, 
        { name: 'CONTRACNO', type: 'string' }, 
        { name: 'CASEFROM', type: 'string' }, 
        { name: 'INSUAMOUNT1', type: 'string' }, 
        { name: 'PAYAMOUNT1', type: 'string' }, 
        { name: 'BUYORDERFLAG', type: 'string' }, 
        { name: 'CARRYKINDI', type: 'string' }, 
        { name: 'CARRYKINDO', type: 'string' }, 
        { name: 'AGGREGATECODE', type: 'string' }, 
        { name: 'ORIGINALPRODUCER', type: 'string' }, 
        { name: 'AGENTNAME', type: 'string' }, 
        { name: 'SPECNUNIT', type: 'string' }, 
        { name: 'ATTACHUNIT', type: 'string' }, 
        { name: 'STOCKUNIT', type: 'string' }, 
        { name: 'UDSERVICEFLAG', type: 'string' }, 
        { name: 'LIMITEDQTYO', type: 'string' }, 
        { name: 'LIMITEDQTYI', type: 'string' }, 
        { name: 'PATHNO', type: 'string' }, 
        { name: 'PUBLICDRUGCODE', type: 'string' }, 
        { name: 'DRUGCLASS', type: 'string' }, 
        { name: 'RESEARCHDRUGFLAG', type: 'string' }, 
        { name: 'LIMITFLAG', type: 'string' }, 
        { name: 'HOSPEXAMINEQTYFLAG', type: 'string' }, 
        { name: 'RESTRICTTYPE', type: 'string' }, 
        { name: 'MAXTAKETIMES', type: 'string' }, 
        { name: 'MAXQTYO', type: 'string' }, 
        { name: 'MAXDAYSO', type: 'string' }, 
        { name: 'VALIDDAYSO', type: 'string' }, 
        { name: 'MAXQTYI', type: 'string' }, 
        { name: 'MAXDAYSI', type: 'string' }, 
        { name: 'VALIDDAYSI', type: 'string' }, 
        { name: 'FIXPATHNOFLAG', type: 'string' }, 
        { name: 'TAKEKIND', type: 'string' }, 
        { name: 'ANTIBIOTICSCODE', type: 'string' }, 
        { name: 'RESTRICTCODE', type: 'string' }, 
        { name: 'FREQNOI', type: 'string' }, 
        { name: 'FREQNOO', type: 'string' }, 
        { name: 'DOSE', type: 'string' }, 
        { name: 'DOSE1', type: 'string' }, 
        { name: 'ORDERABLEDRUGFORM', type: 'string' }, 
        { name: 'RAREDISORDERFLAG', type: 'string' }, 
        { name: 'SPECIALORDERKIND', type: 'string' }, 
        { name: 'HOSPEXAMINEFLAG', type: 'string' }, 
        { name: 'MAXQTYPERTIME', type: 'string' }, 
        { name: 'MAXQTYPERDAY', type: 'string' }, 
        { name: 'ONLYROUNDFLAG', type: 'string' }, 
        { name: 'UNABLEPOWDERFLAG', type: 'string' }, 
        { name: 'COLDSTORAGEFLAG', type: 'string' }, 
        { name: 'LIGHTAVOIDFLAG', type: 'string' }, 
        { name: 'WEIGHTTYPE', type: 'string' }, 
        { name: 'WEIGHTTYPE1', type: 'string' }, 
        { name: 'DANGERDRUGFLAG', type: 'string' }, 
        { name: 'DANGERDRUGMEMO', type: 'string' }, 
        { name: 'DRUGEXTERIOR', type: 'string' }, 
        { name: 'DRUGENGEXTERIOR', type: 'string' }, 
        { name: 'SYMPTOMCHIN', type: 'string' }, 
        { name: 'SYMPTOMENG', type: 'string' }, 
        { name: 'CHINSIDEEFFECT', type: 'string' }, 
        { name: 'ENGSIDEEFFECT', type: 'string' }, 
        { name: 'CHINATTENTION', type: 'string' }, 
        { name: 'ENGATTENTION', type: 'string' }, 
        { name: 'DOHLICENSENO1', type: 'string' }, 
        { name: 'FDASYMPTOM', type: 'string' }, 
        { name: 'DRUGMEMO', type: 'string' }, 
        { name: 'SUCKLESECURITY', type: 'string' }, 
        { name: 'PREGNANTGRADE', type: 'string' }, 
        { name: 'DRUGPICTURELINK', type: 'string' }, 
        { name: 'DRUGLEAFLETLINK', type: 'string' }, 
        { name: 'TDMFLAG', type: 'string' }, 
        { name: 'TDMFLAG1', type: 'string' }, 
        { name: 'TDMFLAG2', type: 'string' }, 
        { name: 'TDMFLAG3', type: 'string' }, 
        { name: 'TDMFLAG4', type: 'string' }, 
        { name: 'TDMFLAG5', type: 'string' }, 
        { name: 'TDMFLAG6', type: 'string' }, 
        { name: 'TDMFLAG7', type: 'string' }, 
        { name: 'TDMFLAG8', type: 'string' }, 
        { name: 'TDMFLAG9', type: 'string' }, 
        { name: 'TDMFLAG10', type: 'string' }, 
        { name: 'TDMFLAG11', type: 'string' }, 
        { name: 'UDPOWDERFLAG', type: 'string' }, 
        { name: 'MACHINEFLAG', type: 'string' }, 
        { name: 'DRUGPARENTCODE1', type: 'string' }, 
        { name: 'DRUGPARENTCODE2', type: 'string' }, 
        { name: 'DRUGPARENTCODE3', type: 'string' }, 
        { name: 'DRUGPARENTCODE4', type: 'string' }, 
        { name: 'DRUGPACKAGE', type: 'string' }, 
        { name: 'ATCCODE1', type: 'string' }, 
        { name: 'ATCCODE2', type: 'string' }, 
        { name: 'ATCCODE3', type: 'string' }, 
        { name: 'ATCCODE4', type: 'string' }, 
        { name: 'ATCCODE5', type: 'string' }, 
        { name: 'ATCCODE6', type: 'string' }, 
        { name: 'ATCCODE7', type: 'string' }, 
        { name: 'ATCCODE8', type: 'string' }, 
        { name: 'GERIATRIC', type: 'string' }, 
        { name: 'LIVERLIMITED', type: 'string' }, 
        { name: 'RENALLIMITED', type: 'string' }, 
        { name: 'BIOLOGICALAGENT', type: 'string' }, 
        { name: 'BLOODPRODUCT', type: 'string' }, 
        { name: 'FREEZING', type: 'string' }, 
        { name: 'RETURNDRUGFLAG', type: 'string' }

    ]
});