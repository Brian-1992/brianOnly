Ext.define('WEBAPP.model.ME_PCAM', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'PCACODE', type: 'string' }, //PCA固定處方頭(畫面)
        { name: 'PCACODE_TEXT', type: 'string' }, //PCA固定處方頭(紅底顯示)
        { name: 'PCACODE_DISPLAY', type: 'string' }, //PCA固定處方頭(白底顯示)
        { name: 'MMNAME_E', type: 'string' }, //英文品茗(畫面)
        { name: 'DOSE', type: 'string' }, //劑量(畫面)
        { name: 'FREQNO', type: 'string' }, //院內頻率(畫面)
        { name: 'E_PATHNO', type: 'string' }, //使用途徑(畫面)
        { name: 'E_ORDERUNIT', type: 'string' }, //醫囑單位(畫面)
    ]
});