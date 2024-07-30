Ext.define('WEBAPP.model.BD0011', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'DOCID', type: 'string' }, //識別號(畫面)
        { name: 'AGEN_NO', type: 'string' }, //廠商代碼(畫面)
        { name: 'MSG', type: 'string' }, //訊息內容(畫面)
        { name: 'OPT', type: 'string' }, //訊息發送註記; A-全部,P-部份
        { name: 'OPT_TEXT', type: 'string' }, //訊息發送註記; A-全部,P-部份
        { name: 'OPT_DISPLAY', type: 'string' }, //訊息發送註記; A-全部,P-部份(畫面)
        { name: 'SEND_DT', type: 'string' }, //通知日期; YYYYMMDD(畫面)
        { name: 'SEND_DT_DISPLAY', type: 'string' }, //通知日期; YYYYMMDD
        { name: 'THEME', type: 'string' }, //訊息主旨(畫面)
        { name: 'STATUS', type: 'string' }, //轉檔識別
        { name: 'STATUS_NAME', type: 'string' }, //轉檔識別; 80-未通知,84-待傳MAIL, 82-已傳MAIL(畫面)
        { name: 'FILENAME', type: 'string' }, //附件
        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});