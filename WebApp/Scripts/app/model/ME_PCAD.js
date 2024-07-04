Ext.define('WEBAPP.model.ME_PCAD', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'PCACODE', type: 'string' }, //PCA固定處方頭
        { name: 'MMCODE', type: 'string' }, //院內碼(畫面)
        { name: 'MMCODE_TEXT', type: 'string' }, //院內碼(紅底顯示)
        { name: 'MMCODE_DISPLAY', type: 'string' }, //院內碼(白底顯示)
        { name: 'MMNAME_E', type: 'string' }, //英文品名(畫面)
        { name: 'DOSE', type: 'string' }, //劑量(畫面)
        { name: 'CONSUMEFLAG', type: 'string' }, //扣庫(畫面)
        { name: 'CONSUMEFLAG_TEXT', type: 'string' }, //扣庫(白框顯示)
        { name: 'COMPUTECODE', type: 'string' }, //計費規則(畫面)
        { name: 'COMPUTECODE_TEXT', type: 'string' }, //計費規則(白框顯示)
        { name: 'E_ORDERUNIT', type: 'string' }, //醫囑單位(畫面)
    ]
});