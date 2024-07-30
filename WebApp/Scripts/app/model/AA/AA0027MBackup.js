Ext.define('WEBAPP.model.AA.AA0027MBackup', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'DOCNO', type: 'string' }, //調帳單號(畫面)
        { name: 'FLOWID', type: 'string' }, //狀態代碼
        { name: 'FLOWID_NAME', type: 'string' }, //狀態名稱(畫面)
        { name: 'UPDATE_TIME', type: 'string' }, //調帳日期(畫面)
        { name: 'FRWH', type: 'string' }, //調帳庫別
        { name: 'FRWH_NAME', type: 'string' }, //調帳庫別(畫面)
        { name: 'FRWH_CODE', type: 'string' }, //調帳庫別(紅底顯示)
        { name: 'APPLY_NOTE', type: 'string' }, //備註(畫面)
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'APPTIME', type: 'string' }, //申請時間(畫面)
        { name: 'APPTIME_TEXT', type: 'string' }, //申請時間(白框顯示)
        { name: 'APP_ID_NAME', type: 'string' } //建立人員(畫面)
    ]
});