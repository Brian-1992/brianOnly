Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var WhnoComboGet = '../../../api/CE0021/GetWhnoCombo';
    var T1Name = '藥局複盤管理作業';
    //var reportUrl = '/Report/C/CE0002.aspx';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var windowHeight = $(window).height();
    //var userId, userName, userInid, userInidName;
    var T1cell = '';
    var T1LastRec = null;
    var loadT22 = false;
    var windowNewOpen = true;
    var todayDateString = '';

    var viewModel = Ext.create('WEBAPP.store.CE0021VM');


    function getTodayDate() {
        Ext.Ajax.request({
            url: '/api/CE0002/CurrentDate',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    todayDateString = data.msg;
                    setComboData();
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getTodayDate();

    function isEditable(chk_ym) {
        if (chk_ym.substring(0, 5) != todayDateString) {
            return false;
        } else {
            return true;
        }
    }
    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: WhnoComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    whnoQueryStore.add({ WH_NO: '', WH_KIND: '', WH_NAME: '', WH_GRADE: '' });
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add(wh_nos[i]);
                        }
                        T1Query.getForm().findField('P0').setValue(wh_nos[0].WH_NO);
                            T1Load(true);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
   

    var T1Store = viewModel.getStore('MasterAll');
    function T1Load(clearMsg) {
        
        if (clearMsg) {
            msglabel('');
        }

        var p0 = T1Query.getForm().findField('P0').getValue();
        if (p0 == null) {
            p0 = "";
        }

        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').rawValue);
        T1Store.getProxy().setExtraParam("p2", userId);
        T1Tool.moveFirst();

    }

    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;

    //#region 主畫面

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            store: whnoQueryStore,
                            name: 'P0',
                            id: 'P0',
                            fieldLabel: '庫房代碼',
                            displayField: 'WH_NAME',
                            valueField: 'WH_NO',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: true,
                            typeAhead: true,
                            forceSelection: true,
                            triggerAction: 'all',
                            multiSelect: false,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
                        },
                        {
                            xtype: 'monthfield',
                            fieldLabel: '盤點年月',
                            name: 'P1',
                            id: 'P1',
                            enforceMaxLength: true,
                            //maxLength: 5,
                            //minLength: 5,
                            //regexText: '請填入民國年月',
                            //regex: /\d{5,5}/,
                            labelWidth: mLabelWidth,
                            width: 180,
                            padding: '0 4 0 4',
                            //format: 'Xm',
                            value: new Date(),
                            listeners: {
                                change: function () {
                                    if (isEditable(T1Query.getForm().findField('P1').rawValue)) {
                                        Ext.getCmp('btnAdd').enable();
                                    } else {
                                        Ext.getCmp('btnAdd').disable();
                                    }
                                }
                            }
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '負責人',
                            name: 'P2',
                            id: 'P2',
                            enforceMaxLength: true,
                            maxLength: 21,
                            labelWidth: 60,
                            width: 190,
                            padding: '0 4 0 4',
                            value: userId + ' ' + userName
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                
                                msglabel('訊息區:');
                                var f = T1Query.getForm();
                                if (!f.findField('P0').getValue() &&
                                    !f.findField('P1').getValue()) {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>與<span style=\'color:red\'>盤點年月</span>至少需填寫一項');
                                    return;
                                }

                                T1Load(true);
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }
                    ]
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        displayInfo: true,
        plain: true,
        buttons: [
            {
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
                handler: function () {
                    var f = T1Query.getForm();
                    if (!f.findField('P0').getValue() ||
                        !f.findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>與<span style=\'color:red\'>盤點年月</span>為必填');
                        return;
                    }

                    T31Query.getForm().findField('T31P0').setValue(T1Query.getForm().findField('P0').rawValue);
                    T31Query.getForm().findField('T31P1').setValue(T1Query.getForm().findField('P1').rawValue);

                    T31Load();
                    insertListWindow.show();
                }
            },
            {
                text: '刪除',
                id: 'btnDelete',
                name: 'btnDelete',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要刪除的盤點單');
                        return;
                    }

                    for (var i = 0; i < selection.length; i++) {
                        if (Number(selection[i].data.CHK_STATUS) > 1 || isNaN(Number(selection[i].data.CHK_STATUS))) {
                            var msg = '盤點單號' + selection[i].data.CHK_NO + '不可刪除，請重新選擇';
                            Ext.Msg.alert('提醒', msg);
                            return;
                        }
                    }

                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        list.push(selection[i].data);
                    }
                    //var r = WEBAPP.model.ME_DOCM.create({
                    //    //APPNAME: 'aaa'
                    //});
                    //T1Store.add(r.copy());
                    //T1Set = '/api/AA0035/MasterCreate';
                    //setFormT1('I', '新增');
                    Ext.MessageBox.confirm('刪除', '刪除盤點單，也會將該盤點單內的目前準備旳盤點品項刪除<br/>是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            //transferToBcwhpick("");
                            deleteMaster(list);
                        }
                    }
                    );
                }
            },
            {
                text: '修改盤點人員',
                id: 'btnMedChangeUid',
                name: 'btnMedChangeUid',
                hidden: false,
                disabled: true,
                handler: function () {
                    if (Number(T1LastRec.data.CHK_STATUS) > 1 || isNaN(Number(T1LastRec.data.CHK_STATUS))) {
                        Ext.Msg.alert('提醒', '盤點單號：' + T1LastRec.data.CHK_NO + ' 不可修改盤點人員');
                        return;
                    }

                    T5Load();
                    var title = T1LastRec.data.CHK_NO + ' ' + T1LastRec.data.WH_NAME + ' ' + T1LastRec.data.CHK_YM + ' ' + T1LastRec.data.CHK_PERIOD_NAME;
                    medChangeUidWindow.setTitle('修改盤點人員 ' + title);

                    medChangeUidWindow.show();
                }
            },
        ]
    });
    function deleteMaster(list) {
        Ext.Ajax.request({
            url: '/api/CE0002/DeleteMaster',
            method: reqVal_p,
            params: { ITEM_STRING: Ext.util.JSON.encode(list), preLevel: '1' },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel("刪除成功");
                    T1Load(false);

                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "庫房代碼",
                dataIndex: 'WH_NAME',
                width: 120
            },
            {
                text: "盤點年月",
                dataIndex: 'CHK_YM',
                width: 70
            },
            {
                text: "庫房級別",
                dataIndex: 'CHK_WH_GRADE',
                width: 70
            },
            //{
            //    text: "庫房分類",
            //    dataIndex: 'WH_KIND_NAME',
            //    width: 80
            //},
            {
                text: "盤點期",
                dataIndex: 'CHK_PERIOD_NAME',
                width: 70
            },
            {
                text: "盤點類別",
                dataIndex: 'CHK_TYPE',
                width: 120,
                xtype: 'templatecolumn',
                tpl: '{CHK_TYPE_NAME}'
            },
            {
                text: "盤點量/總量",
                dataIndex: 'aaa',
                width: 120,
                xtype: 'templatecolumn',
                tpl: '{CHK_NUM}/{CHK_TOTAL}'
            },
            {
                text: "盤點單號",
                dataIndex: 'CHK_NO',
                width: 120,
                renderer: function (val, meta, record) {
                    var CHK_NO = record.data.CHK_NO;
                    return '<a href=javascript:void(0)>' + CHK_NO + '</a>';
                },
            },
            //{
            //    text: "負責人員",
            //    dataIndex: 'CHK_KEEPER_NAME',
            //    width: 120
            //},
            {
                text: "狀態",
                dataIndex: 'CHK_STATUS_NAME',
                width: 80
            },
            {
                text: "已分配盤點人員",
                dataIndex: 'CHK_UID_NAMES',
                flex: 1
            },
        ],
        listeners: {
            //click: {
            //    element: 'el',
            //    fn: function () {

            //    }
            //},
            //itemclick: function (self, record, item, index, e, eOpts) {
            //    msglabel('');

            //    //if (T1cell == '')
            //    
            //    T1Rec = record;
            //},
            selectionchange: function (model, records) {
                T1Rec = records.length;

                if (records.length > 0) {
                    T1LastRec = Ext.clone(records[0]);
                }
                Ext.getCmp('btnMedChangeUid').disable();
                if (records.length == 1) {
                    if (T1LastRec.data.CHK_STATUS == '1') {
                        if (isEditable(T1LastRec.data.CHK_YM)) {
                            Ext.getCmp('btnMedChangeUid').enable();
                        }

                    }
                    return;
                }
            },
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {

                var columns = T1Grid.getColumns();
                var index = getColumnIndex(columns, 'CHK_NO');

                if (index != cellIndex) {
                    return;
                }
                // T61LastRec = record;
                T1cell = 'cell';
                

                T1LastRec = record;

                if (Number(T1LastRec.data.CHK_STATUS) == 4) {
                    T41Load();

                    var title = record.data.CHK_NO + ' ' + record.data.WH_NAME + ' ' + record.data.CHK_YM + ' ' + record.data.CHK_PERIOD_NAME + ' ' + record.data.WH_KIND_NAME
                    returnWindow.setTitle('盤點明細管理 ' + title);

                    returnWindow.show();
                    return;
                }

                if(Number(T1LastRec.data.CHK_STATUS) > 0) {
                    T21Grid.getColumns()[0].hide();
                    T22Grid.getColumns()[0].hide();

                    Ext.getCmp('T21P8').disable();
                    Ext.getCmp('predateSet').disable();

                    Ext.getCmp('btnRemoveFrInclude').disable();
                    //Ext.getCmp('btnSave').disable();
                    Ext.getCmp('btnCreateSheet').disable();
                    Ext.getCmp('btnAddToInclude').disable();
                    //Ext.getCmp('btnPrint').enable();

                    T21Grid.setHeight(windowHeight - 90);
                    T22Grid.setHeight(0);
                } else {
                    T21Grid.getColumns()[0].show();
                    T22Grid.getColumns()[0].show();

                    Ext.getCmp('T21P8').enable();
                    Ext.getCmp('predateSet').enable();

                    Ext.getCmp('btnRemoveFrInclude').enable();
                    Ext.getCmp('btnCreateSheet').enable();
                    Ext.getCmp('btnAddToInclude').enable();

                    T21Grid.setHeight(windowHeight / 2 - 61);
                    T22Grid.setHeight(windowHeight / 2 - 61);
                }

                var t22form = T22Form.getForm();
                t22form.findField('F_U_PRICE').setValue(0);

                var loadT22 = (Number(T1LastRec.data.CHK_STATUS) == 0);

                T21Load(loadT22);
                T23Load();

                var f = T22Form.getForm();
                f.findField('F_U_PRICE').setValue(0);
                f.findField('F_NUMBER').setValue(0);
                f.findField('F_AMOUNT').setValue(0);
                f.findField('ORI_F_U_PRICE').setValue(0);
                f.findField('ORI_F_NUMBER').setValue(0);
                f.findField('ORI_F_AMOUNT').setValue(0);

                //if (Number(T1LastRec.data.CHK_STATUS) == 0) {
                //    //T22Load();
                //    T23Load();
                //}

                //if (record.data.CHK_WH_KIND == "0") {
                //    Ext.getCmp('btnDistribute').enable();
                //} else {
                //    Ext.getCmp('btnDistribute').disable();
                //}
                var title = record.data.CHK_NO + ' ' + record.data.WH_NAME + ' ' + record.data.CHK_YM + ' ' + record.data.CHK_PERIOD_NAME + ' ' + record.data.WH_KIND_NAME
                detailWindow.setTitle('盤點明細管理 ' + title);

                detailWindow.show();
            },
        }
    });
    //#endregion

    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }

    // #region 盤點項目編輯
    var T21Store = viewModel.getStore('DetailInclude');
    function T21Load(isLoadT22) {
        loadT22 = isLoadT22;

        T21Store.removeAll();
        
        
        T21Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T21Store.getProxy().setExtraParam("chk_status", T1LastRec.data.CHK_STATUS);

        T21Tool.moveFirst();
    }
    T21Store.on('load', function (store, options) {
        if (loadT22) {
            T22Store.removeAll();
            T22Load(true);
        }
    });
    var T22Store = viewModel.getStore('DetailExclude');
    function T22Load() {
        var f = T22Form.getForm();
        T22Store.getProxy().setExtraParam("chk_no1", T1LastRec.data.CHK_NO1);
        T22Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T22Store.getProxy().setExtraParam("F_U_PRICE", f.findField('F_U_PRICE').getValue());
        T22Store.getProxy().setExtraParam("F_NUMBER", f.findField('F_NUMBER').getValue());
        T22Store.getProxy().setExtraParam("F_AMOUNT", f.findField('F_AMOUNT').getValue());
        T22Store.getProxy().setExtraParam("MISS_PER", f.findField('MISS_PER').getValue());
        
        T22Tool.moveFirst();
    }
    var T23Store = viewModel.getStore('PickUsers');
    function T23Load() {

        T23Store.getProxy().setExtraParam("wh_no", T1LastRec.data.CHK_WH_NO);

        T23Tool.moveFirst();
    }

    var T21Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP32',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'datefield',
                            fieldLabel: '預計盤點日期',
                            name: 'T21P8',
                            id: 'T21P8',
                            labelWidth: 80,
                            width: 180,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                            minValue: new Date()
                        },
                        {
                            xtype: 'button',
                            text: '設定',
                            id: 'predateSet',
                            handler: function () {
                                
                                if (T21Form.getForm().findField('T21P8').getValue() == null || T21Form.getForm().findField('T21P8').getValue() == undefined) {
                                    Ext.Msg.alert('提醒', '請選擇預計盤點日期');
                                    return;
                                }
                                if (T21Form.getForm().findField('T21P8').isValid() == false) {
                                    Ext.Msg.alert('提醒', '日期小於今天或格式錯誤，請重新選擇');
                                    return;
                                }

                                var selections = T21Grid.getSelection();

                                if (selections.length == 0) {
                                    Ext.Msg.alert('提醒', '請選擇欲設定日期之項目');
                                    return;
                                }
                                var chk_pre_date = getDateFormat(T21Form.getForm().findField('T21P8').getValue());

                                var data = [];
                                for (var i = 0; i < selections.length; i++) {
                                    var item = selections[i].data;
                                    item.CHK_PRE_DATE = chk_pre_date;
                                    data.push(selections[i].data);
                                }

                                var myMask = new Ext.LoadMask(Ext.getCmp('detailWindow'), { msg: '處理中...' });
                                myMask.show();
                                Ext.Ajax.request({
                                    url: '/api/CE0002/SetPreDate',
                                    method: reqVal_p,
                                    params: {
                                        item_string: Ext.util.JSON.encode(data)
                                    },
                                    success: function (response) {
                                        myMask.hide();
                                        T21Load(false);
                                        msglabel("設定預計盤點時間成功");
                                    },
                                    failure: function (response, options) {
                                        Ext.Msg.alert('失敗', '發生例外錯誤');
                                    }
                                });
                            }
                        },
                    ]
                }
            ]
        }]
    });
    function getDateFormat(value) {
        
        var yyyy = value.getFullYear().toString();
        var m = value.getMonth() + 1;
        var d = value.getDate();
        var mm = m > 9 ? m.toString() : "0" + m.toString();
        var dd = d > 9 ? d.toString() : "0" + d.toString();
        return yyyy + "-" + mm + "-" + dd;
    }

    var T22Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP23',
                    border: false,
                    layout: 'vbox',
                    items: [
                        {
                            name: 'ORI_F_U_PRICE',
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_F_NUMBER',
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_F_AMOUNT',
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_MISS_PER',
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '單價 ≧',
                            name: 'F_U_PRICE',
                            enforceMaxLength: false,
                            maxLength: 14,
                            hideTrigger: true,
                            labelSeparator: '',
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '誤差數量  ≧',
                            name: 'F_NUMBER',
                            enforceMaxLength: false,
                            maxLength: 14,
                            hideTrigger: true,
                            labelSeparator: '',
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '誤差金額  ≧',
                            name: 'F_AMOUNT',
                            enforceMaxLength: false,
                            labelSeparator: '',
                            maxLength: 14,
                            hideTrigger: true,
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '誤差百分比  ≧',
                            name: 'MISS_PER',
                            enforceMaxLength: false,
                            labelSeparator: '',
                            maxLength: 14,
                            hideTrigger: true,
                            value: 0
                        }
                    ]
                },

            ]
        }]
    });
    // 印表排序清單
    var printStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
            { "VALUE": "mmcode, store_loc, chk_uid", "TEXT": "院內碼 + 儲位 + 盤點人員" },
            { "VALUE": "mmcode, chk_uid, store_loc", "TEXT": "院內碼 + 盤點人員 + 儲位" },
            { "VALUE": "store_loc, mmcode, chk_uid", "TEXT": "儲位 + 院內碼 + 盤點人員" },
            { "VALUE": "store_loc, chk_uid, mmcode", "TEXT": "儲位 + 盤點人員 + 院內碼" },
            { "VALUE": "chk_uid, mmcode, store_loc", "TEXT": "盤點人員 + 院內碼 + 儲位" },
            { "VALUE": "chk_uid, store_loc, mmcode", "TEXT": "盤點人員 + 儲位 + 院內碼" },
        ]
    });
    var T24Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP25',
                    border: false,
                    layout: 'hbox',
                    width: 250,
                    items: [
                        {
                            xtype: 'combo',
                            store: printStore,
                            name: 'print_order',
                            id: 'print_order',
                            fieldLabel: '印表排序',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: false,
                            typeAhead: true,
                            forceSelection: true,
                            triggerAction: 'all',
                            multiSelect: false,
                            width: '100%',
                            fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                        },
                    ]
                },
            ]
        }]
    });
    var T21Tool = Ext.create('Ext.PagingToolbar', {
        store: T21Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [

            {
                text: '移除',
                id: 'btnRemoveFrInclude',
                name: 'btnRemoveFrInclude',
                handler: function () {
                    var selection = T21Grid.getSelection();

                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇欲移除之項目');
                        return;
                    }

                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        var data = selection[i].data;
                        data.CHK_NO = T1LastRec.data.CHK_NO;
                        list.push(data);
                    }

                    Ext.Ajax.request({
                        url: '/api/CE0021/DeleteG2Whinv',
                        method: reqVal_p,
                        params: {
                            item_string: Ext.util.JSON.encode(list)
                        },
                        success: function (response) {
                            T21Load(true);
                        },
                        failure: function (response, options) {
                            Ext.Msg.alert('失敗', '發生例外錯誤');
                        }
                    });
                }
            },
            {
                text: '產生盤點單',
                id: 'btnCreateSheet',
                name: 'btnCreateSheet',
                handler: function () {
                    if (T21Grid.getStore().data.items.length == 0) {
                        Ext.Msg.alert('提醒', '已選定盤點清單為空，請先選擇項目加入清單');
                        return;
                    }

                    var datas = T21Grid.getStore().data.items;
                    for (var i = 0; i < datas.length; i++) {
                        if (datas[i].data.CHK_PRE_DATE == null) {
                            Ext.Msg.alert('提醒', '請先設定所有項目之預計盤點日期');
                            return;
                        }
                    }

                    T23Grid.getSelectionModel().deselectAll();
                    pickUserWindow.show();

                    
                }
            },
            //{
            //    text: '列印盤點單',
            //    id: 'btnPrint',
            //    name: 'btnPrint',
            //    handler: function () {
            //        //var items = T21Grid.getStore().data.items;
            //        //detailSave();
            //        printWindow.show();
            //    }
            //}
        ]
    });
    var T22Tool = Ext.create('Ext.PagingToolbar', {
        store: T22Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '加入',
                id: 'btnAddToInclude',
                name: 'btnAddToInclude',
                handler: function () {
                    var selection = T22Grid.getSelection();
                    ;
                    //if (T1LastRec.data.CHK_WH_KIND == "1") {
                    //    for (var i = 0; i < selection.length; i++) {
                    //        if (selection[i].data.CHK_UID == null || selection[i].data.CHK_UID.trim() == "") {
                    //            Ext.Msg.alert('提醒', '所選品項未設定管理人員，請先設定');
                    //            return;
                    //        }
                    //    }
                    //}
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇欲加入盤點之項目');
                        return;
                    }

                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        var data = selection[i].data;
                        data.CHK_NO = T1LastRec.data.CHK_NO;
                        list.push(data);
                    }

                    Ext.Ajax.request({
                        url: '/api/CE0021/AddG2Whinv',
                        method: reqVal_p,
                        params: {
                            list: Ext.util.JSON.encode(list)
                        },
                        success: function (response) {
                            T21Load(true);
                        },
                        failure: function (response, options) {
                            Ext.Msg.alert('失敗', '發生例外錯誤');
                        }
                    });

                }
            },
            {
                text: '篩選設定',
                id: 'btnAddFilter',
                name: 'btnAddFilter',
                handler: function () {
                    filterWindow.show();
                }
            },
        ]
    });
    var T23Tool = Ext.create('Ext.PagingToolbar', {
        store: T23Store,
        border: false,
        plain: true,
        displayInfo: true,
    });
    var T21Grid = Ext.create('Ext.grid.Panel', {
        store: T21Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight / 2 - 65,
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T21Form]
            },
            // T21Query
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T21Tool]
            }
        ],
        columns: [
            {
                text: "",
                dataIndex: 'SEQ',
                width: 40,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 120
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "預計盤點日期",
                dataIndex: 'CHK_PRE_DATE',
                width: 100,
            }
        ],
    });
    var T22Grid = Ext.create('Ext.grid.Panel', {
        store: T22Store,
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight / 2 - 62,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T22Tool]
            }
        ],
        columns: [
            {
                text: "",
                dataIndex: 'SEQ',
                width: 40,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 120
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 90,
            },
            {
                text: "電腦量",
                dataIndex: 'STORE_QTY',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY1',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "盤差",
                dataIndex: 'GAP_T',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "誤差比",
                dataIndex: 'MISS_PER',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            }
        ],
    });
    var T23Grid = Ext.create('Ext.grid.Panel', {
        store: T23Store,
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'SIMPLE',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return true;
            }
        },
        selType: 'checkboxmodel',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 400,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T23Tool]
            }
        ],
        columns: [
            //{
            //    text: "員工代碼",
            //    dataIndex: 'WH_CHKUID',
            //    width: 120
            //},
            {
                text: "中文姓名",
                dataIndex: 'WH_CHKUID_NAME',
                width: 120
            },
            {
                header: "",
                flex: 1
            }

        ],
    });

    var detailSave = function (gridStore) {
        
        var list = [];
        for (var i = 0; i < gridStore.length; i++) {
            var item = gridStore[i].data;
            item.CHK_NO = T1LastRec.data.CHK_NO;
            item.CHK_NO1 = T1LastRec.data.CHK_NO1;
            list.push(item);
        }

        if (list.length == 0) {
            list.push({ CHK_NO: T1LastRec.data.CHK_NO });
        }

        Ext.Ajax.request({
            url: '/api/CE0005/DetailSave',
            method: reqVal_p,
            params: {
                chk_wh_grade: T1LastRec.data.CHK_WH_GRADE,
                chk_wh_kind: T1LastRec.data.CHK_WH_KIND,
                chk_no: T1LastRec.data.CHK_NO
            },
            success: function (response) {
                windowNewOpen = true;
                T21Load(true);
                //T22Load();

                msglabel("暫存明細成功");

            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });

    }

    var createSheet = function (detailStore, userStore, wh_kind) {
        
        var users = [];
        for (var i = 0; i < userStore.length; i++) {
            users.push(userStore[i].data);
        }
        

        var myMask = new Ext.LoadMask(Ext.getCmp('pickUserWindow'), { msg: '處理中...' });
        myMask.show();
        var myMask1 = new Ext.LoadMask(Ext.getCmp('detailWindow'), { msg: '處理中...' });
        myMask1.show();

        Ext.Ajax.request({
            url: '/api/CE0021/CreateSheet',
            method: reqVal_p,
            params: {
                //users: Ext.util.JSON.encode(users),
                //wh_kind: wh_kind,
                chk_ym: T1LastRec.data.CHK_YM,
                chk_no: T1LastRec.data.CHK_NO,
                users: Ext.util.JSON.encode(users),
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success == false) {
                    Ext.Msg.alert('提醒', data.msg);
                    myMask.hide();
                    myMask1.hide();
                    return; 
                }

                myMask.hide();
                myMask1.hide();

                T21Load(true);
                T1Load(false);

                msglabel("產生盤點單成功");

                detailWindow.hide();
                pickUserWindow.hide();
                returnWindow.hide();

                T21Grid.getColumns()[0].hide();
                T22Grid.getColumns()[0].hide();
                Ext.getCmp('btnRemoveFrInclude').disable();
                //Ext.getCmp('btnSave').disable();
                Ext.getCmp('btnCreateSheet').disable();
                Ext.getCmp('btnAddToInclude').disable();

                //T21Query.getForm().findField('T21P4').setValue('1 盤中');

            },
            failure: function (response, options) {
                myMask.hide();
                myMask1.hide();
                // Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    var detailWindow = Ext.create('Ext.window.Window', {
        id:'detailWindow',
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [
                    //T21Query,
                    {
                        xtype: 'panel',
                        itemId: 't21Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '已選定盤點清單',
                        border: false,
                        height: '50%',
                        minHeight: windowHeight / 2 - 90,
                        items: [T21Grid]
                    },
                    {
                        xtype: 'splitter',
                        collapseTarget: 'dev'
                    },
                    //T22Query,
                    {
                        xtype: 'form',
                        autoScroll: true,
                        itemId: 't22Grid',
                        region: 'south',
                        layout: 'fit',
                        //collapsible: true,
                        title: '未選定盤點清單',
                        //titleCollapse: true,
                        border: false,
                        height: '50%',
                        minHeight: 30,
                        split: true,
                        items: [T22Grid]
                    }
                ],
            }
        ],

        //items: [T21Grid, T22Grid],
        width: "900px",
        height: windowHeight,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "盤點明細管理",
        //listeners: {
        //    close: function (panel, eOpts) {
        //        myMask.hide();
        //    }
        //}
        buttons: [{
            text: '關閉',
            handler: function () {
                detailWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                detailWindow.center();
            }
        }
    });
    detailWindow.hide();



    var filterWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T22Form],
        resizable: false,
        draggable: true,
        closable: false,
        title: "篩選條件設定",
        buttons: [
            {
                text: '確定',
                handler: function () {

                    var f = T22Form.getForm();
                    if (f.findField('F_U_PRICE').getValue() == null) {
                        f.findField('F_U_PRICE').setValue(0);
                    }
                    if (f.findField('F_NUMBER').getValue() == null) {
                        f.findField('F_NUMBER').setValue(0);
                    }
                    if (f.findField('F_AMOUNT').getValue() == null) {
                        f.findField('F_AMOUNT').setValue(0);
                    }
                    if (f.findField('MISS_PER').getValue() == null) {
                        f.findField('MISS_PER').setValue(0);
                    }

                    f.findField('ORI_F_U_PRICE').setValue(f.findField('F_U_PRICE').getValue());
                    f.findField('ORI_F_NUMBER').setValue(f.findField('F_NUMBER').getValue());
                    f.findField('ORI_F_AMOUNT').setValue(f.findField('F_AMOUNT').getValue());
                    f.findField('ORI_MISS_PER').setValue(f.findField('MISS_PER').getValue());

                    T22Store.getProxy().setExtraParam("F_U_PRICE", f.findField('F_U_PRICE').getValue());
                    T22Store.getProxy().setExtraParam("F_NUMBER", f.findField('F_NUMBER').getValue());
                    T22Store.getProxy().setExtraParam("F_AMOUNT", f.findField('F_AMOUNT').getValue());
                    T22Store.getProxy().setExtraParam("MISS_PER", f.findField('MISS_PER').getValue());
                    T22Load(true);

                    filterWindow.hide();
                }
            },
            {
                text: '取消',
                handler: function () {
                    var f = T22Form.getForm();
                    if (f.findField('F_U_PRICE').getValue() == null) {
                        f.findField('F_U_PRICE').setValue(0);
                    }
                    if (f.findField('F_NUMBER').getValue() == null) {
                        f.findField('F_NUMBER').setValue(0);
                    }
                    if (f.findField('F_AMOUNT').getValue() == null) {
                        f.findField('F_AMOUNT').setValue(0);
                    }
                    if (f.findField('MISS_PER').getValue() == null) {
                        f.findField('MISS_PER').setValue(0);
                    }

                    f.findField('F_U_PRICE').setValue(f.findField('ORI_F_U_PRICE').getValue());
                    f.findField('F_NUMBER').setValue(f.findField('ORI_F_NUMBER').getValue());
                    f.findField('F_AMOUNT').setValue(f.findField('ORI_F_AMOUNT').getValue());
                    f.findField('MISS_PER').setValue(f.findField('ORI_MISS_PER').getValue());

                    filterWindow.hide();
                }
            }
            //{
            //text: '關閉',
            //handler: function () {
            //    filterWindow.hide();
            //}
            //}
        ]
    });
    filterWindow.hide();

    var pickUserWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id:'pickUserWindow',
        items: [
            T23Grid
        ],
        modal: true,
        //items: [T21Grid, T22Grid],
        width: "400px",
        height: 400,
        resizable: true,
        draggable: true,
        closable: false,
        title: "盤點人員挑選",
        buttons: [
            {
                text: '確定',
                id: 'btnSetPickUser',
                name: 'btnSetPickUser',
                handler: function () {
                    var selection = T23Grid.getSelection();
                    
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇盤點人員');
                        return;
                    }

                    Ext.MessageBox.confirm('產生盤點單', '確定產生盤點單?', function (btn, text) {
                        if (btn === 'yes') {
                            if (T1LastRec.data.CHK_STATUS == '4') {
                                
                                createSheet(T21Grid.getStore().data.items, selection, T1LastRec.data.CHK_WH_KIND);
                            } else {
                                createSheet(T21Grid.getStore().data.items, selection, T1LastRec.data.CHK_WH_KIND);
                            }
                        }
                    }
                    );
                }
            },
            {
            text: '關閉',
            handler: function () {
                pickUserWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                pickUserWindow.center();
            }
        }
    });
    pickUserWindow.hide();

    var printWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T24Form],
        resizable: false,
        draggable: true,
        closable: false,
        title: "列印設定",
        buttons: [
            {
                text: '確定',
                handler: function () {
                    if (!T24Form.getForm().findField('print_order').getValue()) {
                        Ext.Msg.alert('提醒', '請選擇印表排序方式');
                        return;
                    }
                    showReport();
                }
            },
            {
                text: '取消',
                handler: function () {
                    printWindow.hide();
                }
            }
        ]
    });
    printWindow.hide();
    function showReport(record) {
        if (!win) {

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?CHK_NO=' + T1LastRec.data.CHK_NO
                + '&PRINT_ORDER=' + T24Form.getForm().findField('print_order').getValue()
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }

    //#endregion

    //#region 挑選盤點單
    // ------ 挑選盤點單 ------
    var T31Store = viewModel.getStore('MasterInsertList');
    function T31Load() {
        //T21Query.getForm().findField('T1P0').setValue(T1Query.getForm().findField('P0').getValue());
        T31Store.getProxy().setExtraParam("wh_no", T1Query.getForm().findField('P0').getValue());
        T31Store.getProxy().setExtraParam("chk_ym", T1Query.getForm().findField('P1').rawValue);
        T31Store.getProxy().setExtraParam("keeper", userId);

        T31Tool.moveFirst();
    }

    var T31Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP31',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            name: 'T31P0',
                            id: 'T31P0',
                            labelWidth: 60,
                            width: 180,
                            fieldLabel: '庫房代碼',
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點年月',
                            name: 'T31P1',
                            id: 'T31P1',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        }
                    ]
                }
            ]
        }]
    });

    
    var T31Tool = Ext.create('Ext.PagingToolbar', {
        store: T31Store,
        border: false,
        displayInfo: true,
        plain: true,
        buttons: [
            {
                text: '加入複盤',
                id: 'btnAddSecond',
                name: 'btnAddSecond',
                handler: function () {
                    var selection = T31Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要加入複盤的盤點單');
                        return;
                    }

                    //for (var i = 0; i < selection.length; i++) {
                    //    if (selection[i].data.CHK_STATUS != "0") {
                    //        var msg = '盤點單號' + selection[i].data.CHK_NO + '不可刪除，請重新選擇';
                    //        Ext.Msg.alert('提醒', msg);
                    //        return;
                    //    }
                    //}

                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        list.push(selection[i].data);
                    }

                    insertMasters(list);

                }
            }
        ]
    });
    var insertMasters = function (list) {
        Ext.Ajax.request({
            url: '/api/CE0005/InsertMaster',
            method: reqVal_p,
            params: { ITEM_STRING: Ext.util.JSON.encode(list) },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('加入複盤成功');
                    T1Load(false);
                    T31Load();
                } else {
                    Ext.Msg.alert('失敗', '發生例外錯誤');
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    var T31Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T31Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight - 60,
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T31Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T31Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "庫房代碼",
                dataIndex: 'WH_NAME',
                width: 120
            },
            {
                text: "盤點年月",
                dataIndex: 'CHK_YM',
                width: 70
            },
            {
                text: "庫房級別",
                dataIndex: 'CHK_WH_GRADE',
                width: 70
            },
            {
                text: "庫房分類",
                dataIndex: 'WH_KIND_NAME',
                width: 80
            },
            {
                text: "盤點期",
                dataIndex: 'CHK_PERIOD_NAME',
                width: 70
            },
            {
                text: "盤點類別",
                dataIndex: 'CHK_TYPE_NAME',
                width: 120
            },
            {
                text: "盤點量/總量",
                dataIndex: 'aaa',
                width: 120,
                xtype: 'templatecolumn',
                tpl: '{CHK_NUM}/{CHK_TOTAL}'
            },
            {
                text: "盤點單號",
                dataIndex: 'CHK_NO',
                width: 120,
                //renderer: function (val, meta, record) {
                //    var CHK_NO = record.data.CHK_NO;
                //    return '<a href=javascript:void(0)>' + CHK_NO + '</a>';
                //},
            },
            {
                text: "負責人員",
                dataIndex: 'CHK_KEEPER_NAME',
                width: 120
            },
            {
                text: "狀態",
                dataIndex: 'CHK_STATUS_NAME',
                width: 80
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            //click: {
            //    element: 'el',
            //    fn: function () {

            //    }
            //},
            //selectionchange: function (model, records) {
            //    msglabel('');

            //    //if (T1cell == '')
            //    
            //    //T1Rec = records.length;
            //    //T1LastRec = records[0];
            //},
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                if (cellIndex != 9) {
                    return;
                }
            },
        }
    });

    var insertListWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        items: [T31Grid],
        modal: true,
        width: "900px",
        height: windowHeight,
        resizable: false,
        draggable: false,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "盤點單挑單",
        buttons: [{
            text: '關閉',
            handler: function () {
                insertListWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                insertListWindow.setY(0);
            }
        }
    });
    insertListWindow.hide();
    //#endregion

    //#region 重盤
    function clearT41FormFilter() {
        var f = T41Form.getForm();
        f.findField('T41P9').setValue('');
        f.findField('T41P10').setValue('');
        f.findField('T41P11').setValue('');
        f.findField('T41P12').setValue('');
    }
    var T41FormMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'T41P11',
        name: 'T41P11',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        width: 160,
        labelWidth: 60,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/CE0002/GetMMCODECombo',//指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1LastRec.data.CHK_NO  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    var T41FormMMCode2 = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'T41P12',
        name: 'T41P12',
        fieldLabel: '至',
        labelAlign: 'right',
        labelWidth: 7,
        width: 107,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/CE0002/GetMMCODECombo',//指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1LastRec.data.CHK_NO  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    var T41Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP42',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'numberfield',
                            fieldLabel: '項次',
                            name: 'T41P9',
                            id: 'T41P9',
                            labelWidth: 60,
                            width: 110,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                            minValue: 1,
                            hideTrigger: true
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '至',
                            name: 'T41P10',
                            id: 'T41P10',
                            labelSeparator: '',
                            labelWidth: 7,
                            width: 57,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                            minValue: 1,
                            hideTrigger: true
                        },
                        T41FormMMCode,
                        T41FormMMCode2,
                        {
                            xtype: 'container',
                            defaultType: 'checkboxfield',
                            layout: 'hbox',
                            items: [
                                {
                                    boxLabel: '僅顯示未設定日期品項',
                                    name: 'chkpredateNullOnly',
                                    inputValue: 'Y',
                                    id: 'chkpredateNullOnly',
                                    width: 140,
                                    padding: '0 0 0 20'
                                }
                            ]
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            id: 'btnMedDetailQueryLoad',
                            handler: function () {
                                T41Load();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            id: 'btnMedDetailQueryClear',
                            handler: function () {
                                clearT41FormFilter();
                            }
                        },

                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP41',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'datefield',
                            fieldLabel: '預計盤點日期',
                            name: 'T41P1',
                            id: 'T41P1',
                            labelWidth: 80,
                            width: 180,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                            minValue: new Date()
                        },
                        {
                            xtype: 'button',
                            text: '設定',
                            id: 'predateSetR',
                            handler: function () {
                                
                                if (T41Form.getForm().findField('T41P1').getValue() == null || T41Form.getForm().findField('T41P1').getValue() == undefined) {
                                    Ext.Msg.alert('提醒', '請選擇預計盤點日期');
                                    return;
                                }
                                if (T41Form.getForm().findField('T41P1').isValid() == false) {
                                    Ext.Msg.alert('提醒', '日期小於今天或格式錯誤，請重新選擇');
                                    return;
                                }

                                var selections = T41Grid.getSelection();

                                if (selections.length == 0) {
                                    Ext.Msg.alert('提醒', '請選擇欲設定日期之項目');
                                    return;
                                }
                                var chk_pre_date = getDateFormat(T41Form.getForm().findField('T41P1').getValue());

                                var data = [];
                                for (var i = 0; i < selections.length; i++) {
                                    var item = selections[i].data;
                                    item.CHK_PRE_DATE = chk_pre_date;
                                    data.push(selections[i].data);
                                }

                                Ext.Ajax.request({
                                    url: '/api/CE0002/SetPreDate',
                                    method: reqVal_p,
                                    params: {
                                        item_string: Ext.util.JSON.encode(data)
                                    },
                                    success: function (response) {
                                        T41Load(false);
                                        msglabel("設定預計盤點時間成功");
                                    },
                                    failure: function (response, options) {
                                        Ext.Msg.alert('失敗', '發生例外錯誤');
                                    }
                                });
                            }
                        },
                    ]
                }
            ]
        }]
    });
    var T41Store = viewModel.getStore('DetailsR');
    function T41Load() {
        T41Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T41Store.getProxy().setExtraParam("chk_no1", T1LastRec.data.CHK_NO1);

        T41Store.getProxy().setExtraParam("seq1", T41Form.getForm().findField('T41P9').getValue());
        T41Store.getProxy().setExtraParam("seq2", T41Form.getForm().findField('T41P10').getValue());
        T41Store.getProxy().setExtraParam("mmcode1", T41Form.getForm().findField('T41P11').getValue());
        T41Store.getProxy().setExtraParam("mmcode2", T41Form.getForm().findField('T41P12').getValue());
        T41Store.getProxy().setExtraParam('chkpredateNullonly', Ext.getCmp('chkpredateNullOnly').getValue() == true ? 'Y' : 'N');

        T41Tool.moveFirst();
    }
    var T41Tool = Ext.create('Ext.PagingToolbar', {
        store: T41Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '產生盤點單',
                id: 'btnMedCreateSheet',
                name: 'btnMedCreateSheet',
                handler: function () {

                    
                    var datas = T41Grid.getStore().data.items;
                    for (var i = 0; i < datas.length; i++) {
                        if (datas[i].data.CHK_PRE_DATE == null) {
                            Ext.Msg.alert('提醒', '請先設定所有項目之預計盤點日期');
                            return;
                        }
                    }

                    T23Load();
                    pickUserWindow.show();
                }
            }]
    });

    var T41Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T41Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        height: windowHeight - 90,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T41Form]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T41Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                text: "",
                dataIndex: 'SEQ',
                width: 40,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 120
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "預計盤點日期",
                dataIndex: 'CHK_PRE_DATE',
                width: 100,
            }
        ]
    });
    
    var returnWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [
                    //T21Query,
                    {
                        xtype: 'panel',
                        itemId: 't41Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        border: false,
                        height: '50%',
                        minHeight: windowHeight / 2 - 90,
                        items: [T41Grid]
                    }
                ],
            }
        ],

        //items: [T21Grid, T22Grid],
        width: "900px",
        height: windowHeight,
        resizable: false,
        draggable: false,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "盤點明細管理",
        //listeners: {
        //    close: function (panel, eOpts) {
        //        myMask.hide();
        //    }
        //}
        buttons: [{
            text: '關閉',
            handler: function () {
                returnWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                returnWindow.setY(0);
            }
        }
    });
    returnWindow.hide();

    //#endregion


    //#region 修改盤點人員 btnMedChangeUid
    var T5Store = viewModel.getStore('CurrentUids');
    function T5Load() {
        //T41Form.getForm().findField('MMCODE').setValue(T41Form.getForm().findField('MMCODE').getValue().toUpperCase())

        var isWard = "Y";
        if (T1LastRec.data.CHK_WH_GRADE == '2' && T1LastRec.data.CHK_WH_KIND == '0') {
            isWard = "N";
        }

        T5Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T5Store.getProxy().setExtraParam("is_ward", isWard);

        T5Tool.moveFirst();
    }

    var T5Tool = Ext.create('Ext.PagingToolbar', {
        store: T5Store,
        border: false,
        plain: true,
        displayInfo: true
    });

    //#region CheckboxModel
    Ext.define('overrides.selection.CheckboxModel', {
        override: 'Ext.selection.CheckboxModel',

        getHeaderConfig: function () {
            var config = this.callParent();

            if (Ext.isFunction(this.selectable)) {
                config.selectable = this.selectable;
                config.renderer = function (value, metaData, record, rowIndex, colIndex, store, view) {
                    if (this.selectable(record)) {
                        record.selectable = false;
                        return '';
                    }
                    record.selectable = true;
                    return this.defaultRenderer();
                };
                this.on('beforeselect', function (rowModel, record, index, eOpts) {
                    return !this.selectable(record);
                }, this);
            }
            return config;
        },

        updateHeaderState: function () {
            // check to see if all records are selected
            
            var me = this,
                store = me.store,
                storeCount = store.getCount(),
                views = me.views,
                hdSelectStatus = false,
                selectedCount = 0,
                selected, len, i, notSelectableRowsCount = 0;

            if (!store.isBufferedStore && storeCount > 0) {
                hdSelectStatus = true;
                store.each(function (record) {
                    if (!record.selectable) {
                        notSelectableRowsCount++;
                    }
                }, this);
                selected = me.selected;

                for (i = 0, len = selected.getCount(); i < len; ++i) {
                    if (store.indexOfId(selected.getAt(i).id) > -1) {
                        ++selectedCount;
                    }
                }
                hdSelectStatus = storeCount === selectedCount + notSelectableRowsCount;
            }

            if (views && views.length) {
                me.column.setHeaderStatus(hdSelectStatus);
            }
        }
    });
    //#endregion

    var T5Grid = Ext.create('Ext.grid.Panel', {
        store: T5Store,
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'SIMPLE',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return record.data.HAS_ENTRY == 'Y';
            }
        },
        selType: 'checkboxmodel',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 400,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T5Tool]
            }
        ],
        columns: [
            {
                text: "中文姓名",
                dataIndex: 'WH_CHKUID_NAME',
                width: 120
            },
            {
                text: "備註",
                dataIndex: 'HAS_ENTRY',
                flex: 1,
                renderer: function (val, meta, record) {
                    if (record.data.HAS_ENTRY == 'Y') {
                        return '人員已輸入盤點量，無法調整';
                    }
                    return '';
                },
            }
        ],
        viewConfig: {
            stripeRows: true,
            listeners: {
                beforerefresh: function (view) {

                    var store = view.getStore();
                    var model = view.getSelectionModel();
                    var s = [];
                    store.queryBy(function (record) {
                        if (record.get('IS_SELECTED') === 'Y') {
                            s.push(record);
                        }
                    });
                    model.select(s);

                },
                //beforeselect: function (grid, record, index, eOpts) {
                //    
                //    if (record.get('HAS_ENTRY') == 'Y') // && record.get('IS_SELECTED') === 'Y') {//replace this with your logic.
                //        {
                //        return false;
                //    } else {
                //        return true;
                //    }
                //}
            },

        },
        viewready: function (grid) {
            var view = grid.view;

            // record the current cellIndex
            grid.mon(view, {
                uievent: function (type, view, cell, recordIndex, cellIndex, e) {
                    grid.cellIndex = cellIndex;
                    grid.recordIndex = recordIndex;
                }
            });

            grid.tip = Ext.create('Ext.tip.ToolTip', {
                target: view.el,
                delegate: '.x-grid-cell',
                trackMouse: true,
                renderTo: Ext.getBody(),
                listeners: {
                    beforeshow: function updateTipBody(tip) {

                        var has_entry = grid.getStore().getAt(grid.recordIndex).get('HAS_ENTRY');
                        if (has_entry == 'N') {
                            return false;
                        } else {
                            tip.update('人員已輸入盤點量，無法移除');
                        }
                    }
                }
            });

        }
    });

    function changeMedUid() {
        var selection = T5Grid.getSelection();
        var users = [];

        for (var i = 0; i < selection.length; i++) {
            users.push({
                WH_CHKUID: selection[i].data.WH_CHKUID,
                HAS_ENTRY: selection[i].data.HAS_ENTRY,
            });
        }
        var store = T5Grid.getStore().data.items;
        for (var i = 0; i < store.length; i++) {
            if (store[i].data.HAS_ENTRY == 'Y') {
                users.push({
                    WH_CHKUID: store[i].data.WH_CHKUID,
                    HAS_ENTRY: store[i].data.HAS_ENTRY,
                });
            }
        }
        if (users.length == 0) {
            Ext.Msg.alert('提醒', '請至少選擇一名人員');
            return;
        }

        var isWard = "Y";
        if (T1LastRec.data.CHK_WH_GRADE == '2' && T1LastRec.data.CHK_WH_KIND == '0') {
            isWard = "N";
        }
        var myMask = new Ext.LoadMask(Ext.getCmp('medChangeUidWindow'), { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0002/ChangeUid',
            method: reqVal_p,
            params: {
                users: Ext.util.JSON.encode(users),
                chk_no: T1LastRec.data.CHK_NO,
                chk_ym: T1LastRec.data.CHK_YM,
                is_ward: isWard
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();
                    msglabel('盤點人員修改成功');
                    T1Load();
                    medChangeUidWindow.hide();
                }
            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }

    var medChangeUidWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        id: 'medChangeUidWindow',
        items: [T5Grid],
        resizable: true,
        draggable: true,
        closable: false,
        title: "新增項目",
        width: 400,
        buttons: [
            {
                text: '確定',
                handler: function () {
                    changeMedUid();
                }
            },
            {
                text: '取消',
                handler: function () {
                    medChangeUidWindow.hide();
                }
            }
        ],
        listeners: {
            show: function (self, eOpts) {
                medChangeUidWindow.center();
                medChangeUidWindow.setWidth(400);
            }
        }
    });
    medChangeUidWindow.hide();
    //#endregion


    // -------------------------

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [
            {
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

    Ext.on('resize', function () {
        windowWidth = $(window).width();
        windowHeight = $(window).height();
        detailWindow.setHeight(windowHeight);
        T21Grid.setHeight(windowHeight / 2 - 62);
        T22Grid.setHeight(windowHeight / 2 - 60);

        if (detailWindow.hidden == false) {
            if (Number(T1LastRec.data.CHK_STATUS) > 0) {
                T21Grid.setHeight(windowHeight - 90);
                T22Grid.setHeight(0);
            }
        }

        pickUserWindow.center();
        detailWindow.center();

        insertListWindow.setHeight(windowHeight);
        T31Grid.setHeight(windowHeight - 60);
        returnWindow.setHeight(windowHeight);
        T41Grid.setHeight(windowHeight - 60);
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();

    T1Load(true);
});