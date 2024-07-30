Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Get = '/api/AB0052/All'; // 查詢
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "效期回報作業";
    var WhnoComboGet = '../../../api/AB0052/GetWhnoCombo';
    var MmcodeComboGet = '../../../api/AB0052/GetMmcodeCombo/';
    var GetMmdataByMmcode = '/api/AB0052/GetMmdataByMmcode';
    var GetExcel = '../../../api/AB0052/Excel';
    var GetExcelSingle = '../../../api/AB0052/ExcelSingle';
    var GetExcelFail = '../../../api/AB0052/ExcelFail';

    var T1Rec = 0;
    var T1LastRec = null;
    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];

    var windowHeight = $(window).height();
    var windowWidth = $(window).width();

    var tempFailItems = null;

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 院內碼清單
    var mmcodeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 批號清單
    var lotnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 效期清單
    var expdateQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: []
    });

    // 設定狀態
    var statusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": " ", "TEXT": "全部" },
            { "VALUE": "1", "TEXT": "未回報" },
            { "VALUE": "2", "TEXT": "已回報" }
        ]
    });

    var uploadFailStore = Ext.create('Ext.data.Store', {
        fields: ['UPLOAD_ROW_NUMBER', 'MMCODE', 'MMNAME_E', 'REPLY_DATE',
            'LOT_NO', 'EXP_QTY', 'EXP_STAT', 'UPLOAD_FAIL_REASON']
    });


    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var viewModel = Ext.create('WEBAPP.store.AB.AB0052VM');


    function setComboData() {
        Ext.Ajax.request({
            url: WhnoComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                            if (i == 0) {
                                T1Query.getForm().findField('P0').setValue(wh_nos[i].VALUE);
                            }
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var mLabelWidth = 60;
    var mWidth = 200;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            labelAlign: 'right'
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    store: whnoQueryStore,
                    fieldLabel: '庫房代碼',
                    name: 'P0',
                    id: 'P0',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: false, // 欄位為必填
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    fieldCls: 'required',
                    multiSelect: false,
                    blankText: "請選擇庫房代碼",
                    labelWidth: mLabelWidth,
                    width: 220,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },

                {
                    xtype: 'monthfield',
                    fieldLabel: '月份',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    labelWidth: mLabelWidth,
                    width: 180,
                    padding: '0 4 0 4',
                    format: 'Xm'
                },
                {
                    xtype: 'combo',
                    store: statusStore,
                    fieldLabel: '狀態',
                    name: 'P2',
                    id: 'P2',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: true, // 欄位為必填
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    labelWidth: mLabelWidth,
                    width: 150,
                    value: ' ',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = T1Query.getForm();
                        if (f.isValid()) {
                            T1Load(true);
                            T1Grid.down('#transfer').setDisabled(false);
                        } else {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var T1Store = viewModel.getStore('MeExpd');
    T1Store.on('load', function () {
        //先更新toolbar
        Ext.getCmp('toolbar').onLoad();

        var recNew = Ext.create('WEBAPP.model.MeExpd');
        recNew.data.MMNAME_E = '*';
        recNew.data.WH_NO = '*';
        recNew.data.EXP_STAT = '1';
        recNew.data.REPLAY_ID = userId + ' ' + userName;
        T1Store.add(recNew);

    });
    function T1Load(clearMsg) {


        if (clearMsg) {
            msglabel('');
        }
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        id: 'toolbar',
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '載入近效期資料', handler: function () {

                    var f = T1Query.getForm();
                    if (f.isValid() == false) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
                        f.findField("P0").focus();
                        return;
                    }

                    Ext.Ajax.request({
                        url: '/api/AB0052/GetWexpinvsN',
                        method: reqVal_p,
                        params: {
                            wh_no: f.findField('P0').getValue()
                        },
                        success: function (response) {
                            msglabel("載入近效期資料成功");
                            T1Load(false);
                        },
                        failure: function (response, options) {

                        }
                    });

                }
            },
            {
                id: 'export', text: '匯出所有效期',
                handler: function () {

                    if (!T1Query.getForm().findField('P0').getValue() ||
                        !T1Query.getForm().findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>、<span style=\'color:red\'>年月</span>為必填');
                        f.findField("P0").focus();
                        return;
                    }

                    var p = new Array();
                    p.push({ name: 'wh_no', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'exp_date', value: getDateTime(T1Query.getForm().findField('P1').getValue()) });
                    p.push({ name: 'status', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'fileName', value: T1Query.getForm().findField('P0').getValue() + '_' + T1Query.getForm().findField('P1').rawValue + '_' + T1Query.getForm().findField('P2').rawValue + '.xls' });
                    PostForm(GetExcel, p);
                }
            },
            {
                id: 'exportSingle', text: '匯出最近一筆效期',
                handler: function () {

                    if (!T1Query.getForm().findField('P0').getValue() ||
                        !T1Query.getForm().findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>、<span style=\'color:red\'>年月</span>為必填');
                        f.findField("P0").focus();
                        return;
                    }

                    var p = new Array();
                    p.push({ name: 'wh_no', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'exp_date', value: getDateTime(T1Query.getForm().findField('P1').getValue()) });
                    p.push({ name: 'status', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'fileName', value: T1Query.getForm().findField('P0').getValue() + '_' + T1Query.getForm().findField('P1').rawValue + '_' + T1Query.getForm().findField('P2').rawValue + '.xls' });
                    PostForm(GetExcelSingle, p);
                }
            },
            {
                xtype: 'filefield',
                name: 'import',
                id: 'import',
                buttonText: '匯入',
                buttonOnly: true,
                width: 40,
                listeners: {
                    change: function (widget, value, eOpts) {
                        var files = event.target.files;
                        if (!files || files.length == 0) return; // make sure we got something
                        var file = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '僅支援讀取xlsx和xls格式！');
                            Ext.getCmp('import').fileInputEl.dom.value = '';
                            msglabel('');
                        } else {
                            msglabel("已選擇檔案");
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", file);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AB0052/Import",
                                data: formData,
                                timeout: 300000,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    myMask.hide();
                                    if (!data.success) {
                                        T1Store.removeAll();
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('import').fileInputEl.dom.value = '';
                                    }
                                    else {
                                        uploadFailStore.removeAll();
                                        if (data.etts.length > 0) {
                                            tempFailItems = data.etts;
                                            var MMCODEdata = '';
                                            var REPLY_DATEdata = '';
                                            var LOT_NOdata = '';
                                            var EXP_QTYdata = '';
                                            var REPLY_TIMEdata = '';
                                            var UFRdata = '';
                                            for (var i = 0; i < data.etts.length; i++) {
                                                uploadFailStore.add({
                                                    UPLOAD_ROW_NUMBER: data.etts[i].UPLOAD_ROW_NUMBER,
                                                    MMCODE: data.etts[i].MMCODE,
                                                    MMNAME_E: data.etts[i].MMNAME_E,
                                                    REPLY_DATE: data.etts[i].REPLY_DATE,
                                                    LOT_NO: data.etts[i].LOT_NO,
                                                    EXP_QTY: data.etts[i].EXP_QTY,
                                                    EXP_STAT: data.etts[i].EXP_STAT,
                                                    UPLOAD_FAIL_REASON: data.etts[i].UPLOAD_FAIL_REASON
                                                });

                                                if (i == (data.etts.length - 1))
                                                {
                                                    MMCODEdata += data.etts[i].MMCODE;
                                                    REPLY_DATEdata += data.etts[i].REPLY_DATE;
                                                    LOT_NOdata += data.etts[i].LOT_NO;
                                                    EXP_QTYdata += data.etts[i].EXP_QTY;
                                                    REPLY_TIMEdata += data.etts[i].REPLY_TIME;
                                                    UFRdata += data.etts[i].UPLOAD_FAIL_REASON;
                                                }
                                                else
                                                {
                                                    MMCODEdata += data.etts[i].MMCODE + '^';
                                                    REPLY_DATEdata += data.etts[i].REPLY_DATE + '^';
                                                    LOT_NOdata += data.etts[i].LOT_NO + '^';
                                                    EXP_QTYdata += data.etts[i].EXP_QTY + '^';
                                                    REPLY_TIMEdata += data.etts[i].REPLY_TIME + '^';
                                                    UFRdata += data.etts[i].UPLOAD_FAIL_REASON + '^';
                                                }
                                                
                                            }

                                            // 建立含錯誤說明的xls檔
                                            var p = new Array();
                                            p.push({ name: 'wh_no', value: data.etts[0].WH_NO });
                                            p.push({ name: 'exp_date', value: data.etts[0].EXP_DATE });
                                            p.push({ name: 'status', value: '1' });
                                            p.push({ name: 'mmcode', value: MMCODEdata });
                                            p.push({ name: 'reply_date', value: REPLY_DATEdata });
                                            p.push({ name: 'lot_no', value: LOT_NOdata });
                                            p.push({ name: 'exp_qty', value: EXP_QTYdata });
                                            p.push({ name: 'reply_time', value: REPLY_TIMEdata });
                                            p.push({ name: 'ufr', value: UFRdata });
                                            p.push({ name: 'fileName', value: data.etts[0].WH_NO + '_' + data.etts[0].EXP_DATE + '(含錯誤說明)' + '.xls' });
                                            PostForm(GetExcelFail, p);

                                            //T2Grid.bindStore(uploadFailStore);
                                            //T2Grid.getView().refresh();
                                            uploadFailWindow.show();

                                        }

                                        msglabel("訊息區:資料匯入成功");
                                        Ext.getCmp('import').fileInputEl.dom.value = '';
                                    }

                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    myMask.hide();
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                    Ext.getCmp('import').setRawValue("");
                                }
                            });
                        }
                    }

                }

            },
            {
                id: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0052/Delete';
                            var selection = T1Grid.getSelection();
                            list = [];
                            for (var i = 0; i < selection.length; i++) {

                                list.push({
                                    wh_no: selection[i].data.WH_NO,
                                    exp_date: selection[i].data.EXP_DATE,
                                    mmcode: selection[i].data.MMCODE,
                                    exp_seq: selection[i].data.EXP_SEQ,
                                });
                            }

                            Ext.Ajax.request({
                                url: T1Set,
                                method: reqVal_p,
                                params: {
                                    list: Ext.util.JSON.encode(list)
                                },
                                success: function (response) {
                                    msglabel("資料刪除成功");
                                    T1Load(false);
                                },
                                failure: function (response, options) {

                                }
                            });
                        }
                    }
                    );
                }
            },
            {
                itemId: 'transfer', text: '傳送', disabled: true,
                handler: function () {

                    var f = T1Query.getForm();
                    if (f.isValid() == false) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
                        return;
                    }

                    if (f.findField('P1').getValue() == null) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>月份</span>為必填');
                        return;
                    }

                    Ext.MessageBox.confirm('傳送', '是否確定傳送？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0052/Transfer';

                            //Ext.Ajax.timeout = 300000;
                            //Ext.Ajax.setTimeout(300000);

                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();

                            var wh_no = T1Query.getForm().findField('P0').getValue();
                            var exp_date = T1Query.getForm().findField('P1').rawValue + '01';
                            Ext.Ajax.request({
                                url: T1Set,
                                method: reqVal_p,
                                params: {
                                    wh_no: wh_no,
                                    exp_date: exp_date
                                },
                                success: function (response) {

                                    msglabel("資料傳送成功");
                                    myMask.hide();
                                    T1Load(false);
                                },
                                failure: function (response, options) {

                                    Ext.Msg.alert('提醒', '資料傳送失敗');
                                    myMask.hide();
                                }
                            });
                        }
                    });
                }
            }
        ]
    });

    function getToday() {
        var year = (new Date().getFullYear() - 1911).toString();
        var month = (new Date().getMonth() + 1).toString();
        month = month.length < 2 ? "0" + month : month;
        return year + month;
    }
    function getDateTime(dateTime) {
        var year = dateTime.getFullYear().toString();
        var month = (dateTime.getMonth() + 1).toString();
        month = month.length < 2 ? "0" + month : month;
        return year + "-" + month + "-01";
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer',
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            style: 'color:red',
            editor: {
                xtype: 'mmcodecombo',
                name: 'MMCODE',
                fieldCls: 'required',
                selectOnFocus: true,
                allowBlank: false,
                labelAlign: 'right',
                msgTarget: 'side',
                width: 300,
                //width: 150,

                //限制一次最多顯示10筆
                limit: 10,

                //指定查詢的Controller路徑
                queryUrl: '/api/AB0052/GetMMCODECombo',

                //查詢完會回傳的欄位
                extraFields: ['MMNAME_E', 'MMNAME_C', 'STORE_LOC'],

                //查詢時Controller固定會收到的參數
                getDefaultParams: function () {
                    return {
                        wh_no: T1Query.getForm().findField('P0').getValue()
                    };
                },
                listeners: {
                    select: function (c, r, i, e) {
                        msglabel('');
                        //選取下拉項目時，顯示回傳值
                        T1LastRec.set('MMCODE', r.get('MMCODE'));
                        T1LastRec.set('MMNAME_C', r.get('MMNAME_C'));
                        T1LastRec.set('MMNAME_E', r.get('MMNAME_E'));
                        T1LastRec.set('STORE_LOC', r.get('STORE_LOC'));

                        T1LastRec.set('REPLY_DATE', '');
                        T1LastRec.set('LOT_NO', '');
                        T1LastRec.set('EXP_QTY', '');
                        getExpDates(T1Query.getForm().findField('P0').getValue(), r.data.MMCODE);
                    },
                    specialkey: function (field, e) {
                        if (e.getKey() === e.RIGHT) {
                            var editPlugin = this.up().editingPlugin;
                            editPlugin.startEdit(editPlugin.context.rowIdx, 6);
                        }
                    }
                }
            }
        },
        {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 200
        }, {
            text: "回覆效期",
            dataIndex: 'REPLY_DATE',
            width: 80,
            style: 'color:red',
            editor: {
                xtype: 'combo',
                store: expdateQueryStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                fieldCls: 'required',
                allowBlank: false,
                listeners: {

                    select: function (c, r, i, e) {
                        msglabel('');
                        if (r.get('VALUE').trim() != T1LastRec.get('EXP_DATE')) {
                            //選取下拉項目時，顯示回傳值
                            T1LastRec.set('REPLY_DATE', r.get('VALUE'));
                            T1LastRec.set('EXP_QTY', '');

                        }

                        getLotNos(T1Query.getForm().findField('P0').getValue(), T1LastRec.data.MMCODE, T1LastRec.data.REPLY_DATE);
                    },
                    specialkey: function (field, e) {
                        if (e.getKey() === e.RIGHT) {
                            var editPlugin = this.up().editingPlugin;
                            editPlugin.startEdit(editPlugin.context.rowIdx, editPlugin.context.colIdx + 1);
                        }
                    },
                    focus: function (field, eOpts) {
                        setTimeout(() => getExpDates(T1Query.getForm().findField('P0').getValue(), T1LastRec.data.MMCODE), 1000);
                    }
                }
            }
        }, {
            text: "藥品批號",
            dataIndex: 'LOT_NO',
            width: 110,
            style: 'color:red',
            editor: {
                xtype: 'combo',
                store: lotnoQueryStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                fieldCls: 'required',
                allowBlank: false,
                listeners: {
                    select: function (c, r, i, e) {
                        msglabel('');
                        if (r.get('VALUE').trim() != T1LastRec.get('LOT_NO')) {
                            //選取下拉項目時，顯示回傳值
                            T1LastRec.set('LOT_NO', r.get('VALUE'));
                            T1LastRec.set('EXP_QTY', '');

                        }
                    },
                    specialkey: function (field, e) {
                        if (e.getKey() === e.RIGHT) {
                            var editPlugin = this.up().editingPlugin;
                            editPlugin.startEdit(editPlugin.context.rowIdx, editPlugin.context.colIdx + 1);
                        }
                    },
                    focus: function (field, eOpts) {
                        setTimeout(() => getLotNos(T1Query.getForm().findField('P0').getValue(), T1LastRec.data.MMCODE, T1LastRec.data.REPLY_DATE), 1000);

                    }
                }
            }
        }, {
            xtype: 'numbercolumn',
            text: "回覆藥量",
            dataIndex: 'EXP_QTY',
            width: 80,
            format: '0.00',
            align: 'right',
            style: 'text-align:left; color:red',
            editor: {
                fieldCls: 'required',
                selectOnFocus: true,
                fieldStyle: 'text-align:right;',
                allowBlank: false,
                listeners: {
                    specialkey: function (field, e) {
                        if (e.getKey() === e.RIGHT) {
                            var editPlugin = this.up().editingPlugin;
                            editPlugin.startEdit(editPlugin.context.rowIdx, editPlugin.context.colIdx + 1);
                        }

                        if (e.getKey() === e.UP) {
                            var editPlugin = this.up().editingPlugin;
                            editPlugin.completeEdit();
                            var sm = T1Grid.getSelectionModel();
                            sm.deselectAll();
                            sm.select(editPlugin.context.rowIdx - 1);
                            editPlugin.startEdit(editPlugin.context.rowIdx - 1, 6);
                        }

                        if (e.getKey() === e.DOWN) {
                            var editPlugin = this.up().editingPlugin;
                            editPlugin.completeEdit();
                            var sm = T1Grid.getSelectionModel();
                            sm.deselectAll();
                            sm.select(editPlugin.context.rowIdx + 1);
                            editPlugin.startEdit(editPlugin.context.rowIdx + 1, 6);
                        }
                    }
                }
            }

        }, {
            text: "庫存量",
            dataIndex: 'INV_QTY',
            align: 'right',
            style: 'text-align:left',
            width: 100
        },
        {
            text: "藥庫儲位",
            dataIndex: 'STORE_LOC',
            width: 100
        },
        {
            text: "備註",
            dataIndex: 'MEMO',
            width: 150,
            style: 'color:red',
            editor: {
                allowBlank: true,
                listeners: {
                    specialkey: function (field, e) {
                        if (e.getKey() === e.LEFT) {
                            var editPlugin = this.up().editingPlugin;
                            editPlugin.startEdit(editPlugin.context.rowIdx, editPlugin.context.colIdx - 1);
                        }
                    }
                }
            }
        },
        {
            text: "回覆日期",
            dataIndex: 'REPLY_TIME',
            width: 80,

        }, {
            text: "回覆人員",
            dataIndex: 'REPLY_ID',
            width: 130,

        },
        {
            text: "狀態",
            dataIndex: 'EXP_STAT_NAME',
            width: 150,

        },
        ],
        listeners: {
            beforeedit: function (plugin, context) {
                var status = T1LastRec.data.EXP_STAT;
                if (status == '2') {
                    return false;
                }
            },
            selectionchange: function (model, records) {

                T1Rec = records.length;
                T1LastRec = records[0];

                Ext.getCmp('delete').enable();

                for (var i = 0; i < records.length; i++) {
                    if (records[i].data.EXP_STAT == '2') {
                        Ext.getCmp('delete').disable();
                    }
                }
            }
        },
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,
                listeners: {
                    beforeedit: function (editor, context, eOpts) {
                        if (context.record.data.EXP_STAT != '1') {
                            return false;
                        }
                        else {
                            return true;
                        }
                    },
                    edit: function (editor, context, eOpts) {
                        
                        var modifiedRec = context.store.getModifiedRecords();
                        var r = context.record;
                        if (r.get('EXP_STAT') != '1') {
                            return false;
                        }

                        var wh_no = r.get('WH_NO');
                        var reply_date = (context.field === 'REPLY_DATE') ? context.value : r.get('REPLY_DATE');
                        var lot_no = (context.field === 'LOT_NO') ? context.value : r.get('LOT_NO');
                        var exp_qty = (context.field === 'EXP_QTY') ? context.value : r.get('EXP_QTY');
                        switch (context.field) {
                            case 'REPLY_DATE':
                                r.set('REPLY_DATE', context.value);
                                break;
                            case 'LOT_NO':
                                r.set('LOT_NO', context.value);
                                break;
                        }
                        if (wh_no == '*') //新增
                        {

                            if (reply_date == '' || lot_no == '' || exp_qty == '' || isNaN(Number(exp_qty))) return;
                            Ext.each(modifiedRec, function (item) {
                                if (item.crudState === 'C') {
                                    T1LastRec.set('WH_NO', T1Query.getForm().findField('P0').getValue());
                                    Ext.Ajax.request({
                                        url: '/api/AB0052/Add',
                                        method: reqVal_p,
                                        params: T1LastRec.data,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {

                                                msglabel('資料新增成功');
                                                T1Query.getForm().findField('P1').setValue(getToday());
                                                T1Load(false);
                                            }
                                            else {
                                                Ext.Msg.alert('提醒', data.msg);
                                                T1Load(false);
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        },
                                    });
                                }
                            });
                        }
                        else {
                            if (reply_date == '' || lot_no == '' || exp_qty == '' || isNaN(Number(exp_qty))) return;
                            Ext.each(modifiedRec, function (item) {
                                if (item.crudState === 'U') {
                                    Ext.Ajax.request({
                                        url: '/api/AB0052/Update',
                                        method: reqVal_p,
                                        params: T1LastRec.data,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                msglabel('資料更新成功');
                                                T1Load();
                                            }
                                            else {
                                                Ext.Msg.alert('提醒', data.msg);
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        },

                                    });
                                }
                            });
                        }
                    }
                }
            })
        ]
    });

    var MMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'MMCODE',
        name: 'MMCODE',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0076/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                wh_no: T1Query.getForm().findField('P0').getValue(),  //p0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值

                if (T1Query.getForm().findField('P0').isValid()) {
                    getExpDates(T1Query.getForm().findField('P0').getValue(), r.data.MMCODE);

                } else {
                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
                }
            }
        },
    });

    function getExpDates(wh_no, mmcode) {
        Ext.Ajax.request({
            url: '/api/AB0052/GetExpDates',
            method: reqVal_p,
            params: { mmcode: mmcode, wh_no: wh_no },
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success == false) {
                    Ext.Msg.alert('訊息', data.msg);
                    return;
                }
                expdateQueryStore.removeAll();
                var datas = data.etts;
                if (datas.length > 0) {


                    for (var i = 0; i < datas.length; i++) {
                        expdateQueryStore.add({ VALUE: datas[i].VALUE, TEXT: datas[i].TEXT })
                    }

                    if (datas.length == 1) {
                        T1LastRec.set('REPLY_DATE', datas[0].VALUE);
                        getLotNos(wh_no, mmcode, datas[0].VALUE);
                    }
                }
                else {
                    Ext.Msg.alert('訊息', '該院內碼無效期資料');
                    T1LastRec.set('REPLY_DATE', '');
                    T1LastRec.set('LOT_NO', '');
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function getLotNos(wh_no, mmcode, exp_date) {
        Ext.Ajax.request({
            url: '/api/AB0052/GetLotNos',
            method: reqVal_p,
            params: { mmcode: mmcode, wh_no: wh_no, exp_date: exp_date },
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success == false) {
                    Ext.Msg.alert('訊息', data.msg);
                    T1LastRec.set('REPLY_DATE', datas[0].VALUE);
                    return;
                }

                var datas = data.etts;
                if (datas.length > 0) {
                    lotnoQueryStore.removeAll();

                    for (var i = 0; i < datas.length; i++) {
                        lotnoQueryStore.add({ VALUE: datas[i].VALUE, TEXT: datas[i].TEXT })
                    }

                    if (datas.length == 1) {
                        T1LastRec.set('LOT_NO', datas[0].VALUE);
                    }
                }
                else {
                    Ext.Msg.alert('訊息', '該院內碼無效期資料');
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function getExpdItems(wh_no, mmcode, LastRec) {
        Ext.Ajax.request({
            url: '/api/AB0052/GetExpdItems',
            method: reqVal_p,
            params: { mmcode: mmcode, wh_no: wh_no },
            success: function (response) {
                var data = Ext.decode(response.responseText);

                var datas = data.etts;

                lotnoQueryStore.removeAll();
                expdateQueryStore.removeAll();

                for (var i = 0; i < datas.length; i++) {
                    lotnoQueryStore.add({ VALUE: datas[i].LOT_NO, TEXT: datas[i].LOT_NO });


                    if (LastRec.data.LOT_NO == datas[i].LOT_NO) {

                        for (var j = 0; j < datas[i].ExpDateItems.length; j++) {

                            expdateQueryStore.add({
                                VALUE: datas[i].ExpDateItems[j].EXP_DATE,
                                TEXT: datas[i].ExpDateItems[j].EXP_DATE
                            });
                        }
                    }

                }
            },
            failure: function (response, options) {

            }
        });
    }

    //#region 上傳錯誤說明
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: uploadFailStore,
        border: false,
        plain: true,
        displayInfo: true
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: uploadFailStore,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight-60,
        width: windowWidth-30,
        dockedItems: [
            //{
            //    dock: 'top',
            //    xtype: 'toolbar',
            //    items: [T2Tool]
            //}
        ],
        columns: [
            {
                xtype:'rownumberer'
            },
            {
                text: "excel項次",
                dataIndex: 'UPLOAD_ROW_NUMBER',
                width: 80
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200
            },
            {
                text: "回覆效期",
                dataIndex: 'REPLY_DATE',
                width: 110
            },
            {
                text: "藥品批號",
                dataIndex: 'LOT_NO',
                width: 120
            },
            {
                text: "回覆藥量",
                dataIndex: 'EXP_QTY',
                width: 80
            },
            {
                text: "回覆狀態",
                dataIndex: 'EXP_STAT',
                width: 120
            },
            {
                text: "錯誤說明",
                dataIndex: 'UPLOAD_FAIL_REASON',
                width: 120
            },
            {
                header: "",
                flex: 1
            }

        ],
    });
    var uploadFailWindow = Ext.create('Ext.window.Window', {
        id: 'uploadFailWindow',
        renderTo: Ext.getBody(),
        items: [
            T2Grid
        ],
        modal: true,
        resizable: false,
        draggable: false,
        closable: false,
        y: 0,
        title: "上傳失敗說明",
        buttons: [{
            text: '關閉',
            handler: function () {
                uploadFailWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                //console.log(uploadFailStore);
                //console.log(T2Grid.getStore());
                uploadFailWindow.setY(0);
            }
        }
    });
    uploadFailWindow.hide();
    //#endregion

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }
        ]
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中' });

    Ext.on('resize', function () {
        windowWidth = $(window).width();
        windowHeight = $(window).height();
        uploadFailWindow.setHeight(windowHeight);
        //uploadFailWindow.setWidth(windowWidth);
    });

});
