Ext.define('WEBAPP.model.AB0053_2', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' }, //院內碼(畫面)
        { name: 'MMNAME_C', type: 'string' }, //中文品名(畫面)
        { name: 'MMNAME_E', type: 'string' }, //英文品名(畫面)
        { name: 'EXP_DATE1', type: 'string' }, //最近效期(畫面)
        { name: 'LOT_NO', type: 'string' }, //藥品批號(畫面)
        { name: 'REPLY_DATE', type: 'string' }, //回覆效期(畫面)
        { name: 'PH1S', type: 'string' }, //藥庫(畫面)
        { name: 'CHEMO', type: 'string' }, //內湖化療調配室(畫面)
        { name: 'CHEMOT', type: 'string' }, //汀洲化療調配室(畫面)
        { name: 'PH1A', type: 'string' }, //內湖住院藥局(畫面)
        { name: 'PH1C', type: 'string' }, //內湖門診藥局(畫面)
        { name: 'PH1R', type: 'string' }, //內湖急診藥局(畫面)
        { name: 'PHMC', type: 'string' }, //汀洲藥局(畫面)
        { name: 'TPN', type: 'string' }, //製劑室(畫面)
        { name: 'BASE_UNIT', type: 'string' }, //劑量單位(畫面)
        { name: 'E_MANUFACT', type: 'string' }, //廠商(畫面)
    ]
});