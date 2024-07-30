Ext.define('WEBAPP.model.AB0053_3', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MMCODE', type: 'string' }, //院內碼(畫面)
        { name: 'MMNAME_C', type: 'string' }, //中文品名(畫面)
        { name: 'MMNAME_E', type: 'string' }, //英文品名(畫面)
        { name: 'EXP_DATE', type: 'date' }, //有效日期(畫面)
        { name: 'WARNYM', type: 'string' }, //警示效期(年/月)(畫面)
        { name: 'LOT_NO', type: 'string' }, //藥品批號(畫面)
        { name: 'EXP_QTY', type: 'string' }, //數量(畫面)
        { name: 'MEMO', type: 'string' }, //備註(畫面)
        { name: 'CLOSEFLAG', type: 'string' }, //結案否
        { name: 'CREATE_TIME', type: 'string' }, //建立日期
        { name: 'CREATE_USER', type: 'string' }, //建立人員
        { name: 'UPDATE_IP', type: 'string' }, //異動IP
        { name: 'UPDATE_TIME', type: 'string' }, //異動日期
        { name: 'UPDATE_USER', type: 'string' }, //異動人員
        { name: 'MMCODE_TEXT', type: 'string' }, //院內碼(紅底顯示)
        { name: 'MMCODE_DISPLAY', type: 'string' }, //院內碼(白底顯示)
        { name: 'EXP_DATE_TEXT', type: 'string' }, //有效日期(紅底顯示)
        { name: 'EXP_DATE_DISPLAY', type: 'string' }, //有效日期(白底顯示)
        { name: 'WARNYM_TEXT', type: 'string' }, //警示效期(年/月)(紅底顯示)
        { name: 'CLOSEFLAG_NAME', type: 'string' }, //結案否(畫面)
        { name: 'CLOSEFLAG_TEXT', type: 'string' } //結案否(紅底顯示)
    ]
});