Ext.define('WEBAPP.model.AB0053_1', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' }, //院內碼(畫面)
        { name: 'MMNAME_C', type: 'string' }, //中文品名(畫面)
        { name: 'MMNAME_E', type: 'string' }, //英文品名(畫面)
        { name: 'LOT_NO', type: 'string' }, //藥品批號(畫面)
        { name: 'REPLY_DATE', type: 'string' }, //回覆效期(畫面)
        { name: 'EXP_QTY', type: 'string' }, //效期藥量(畫面)
        { name: 'EXP_SEQ', type: 'string' }, //效期流水號(畫面)
        { name: 'EXP_DATE1', type: 'string' }, //最近效期一(畫面)
        { name: 'LOT_NO1', type: 'string' }, //藥品批號1(畫面)
        { name: 'EXP_DATE2', type: 'string' }, //最近效期二(畫面)
        { name: 'LOT_NO2', type: 'string' }, //藥品批號2(畫面)
        { name: 'EXP_DATE3', type: 'string' }, //最近效期三(畫面)
        { name: 'LOT_NO3', type: 'string' }, //藥品批號3(畫面)
        { name: 'MEMO', type: 'string' }, //備註(畫面)
        { name: 'REPLY_TIME', type: 'string' }, //回覆日期(畫面)
        { name: 'REPLY_ID', type: 'string' }, //回覆人員(畫面)
        { name: 'CLOSE_TIME', type: 'string' }, //結案日期(畫面)
        { name: 'CLOSE_ID', type: 'string' }, //截止人員(畫面)
        { name: 'EXP_DATE', type: 'string' }, //有效日期(畫面)
        { name: 'WH_NO', type: 'string' }, //庫別代碼(畫面)
        { name: 'WH_NAME', type: 'string' }, //庫別名稱(畫面)
        { name: 'EXP_STAT', type: 'string' } //效期回覆狀態(畫面)
    ]
});