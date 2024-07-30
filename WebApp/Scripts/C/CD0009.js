Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var WhnoComboGet = '/api/CD0002/GetWhnoCombo';
    var DutyUserComboGet = '/api/CD0002/GetDutyUserCombo';
    var TransferTobcwhpickUrl = '/api/CD0009/TransferToBcwhpick';

    var reportUrl = '/Report/C/CD0009.aspx';
    var reportNoUserUrl = '/Report/C/CD0009_1.aspx';

    var windowWidth = $(window).width();
    var windowHeight = $(window).height();

    var T1LastRec = null;
    var T31LastRec = null;
    var T41LastRec = null;
    var T51LastRec = null;
    var T61LastRec = null;
    var T1Name = '分配揀貨人員';
    var assignType = "=";

    var isDistributed = ''; // =:已分配 <>:待分配
    var T31cell = '';
    var T41cell = '';
    var T51cell = '';
    var T61cell = '';
    var max_temp_lot_no = '';
    var dutyUser = '';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    //var userId, userName, userInid, userInidName;

    var viewModel = Ext.create('WEBAPP.store.CD0009VM');

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME', 'WH_KIND', 'WH_USERID']
    });
    function getWhnoCombo() {
        Ext.Ajax.request({
            url: WhnoComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        
                        //whnoQueryStore.add({ WH_NO: '', WH_NAME: '', WH_KIND: '', WH_USERID:''});
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add({
                                WH_NO: wh_nos[i].WH_NO,
                                WH_NAME: wh_nos[i].WH_NAME,
                                WH_KIND: wh_nos[i].WH_KIND,
                                WH_USERID: wh_nos[i].WH_USERID,
                            });
                        }

                        T1Query.getForm().findField('P0').setValue('PH1S');
                        getDutyUserCombo('PH1S');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getWhnoCombo();

    // 分派人員清單
    var dutyUserGridStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_USERID', 'WH_USER_NAME']
    });
    function getDutyUserCombo(wh_no) {
        dutyUserGridStore.removeAll();
        Ext.Ajax.request({
            url: DutyUserComboGet,
            method: reqVal_p,
            params: { WH_NO: wh_no },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var users = data.etts;
                    if (users.length > 0) {
                        //whnoQueryStore.add({ WH_NO: '', WH_NAME: '', WH_KIND: '', WH_USERID:''});
                        for (var i = 0; i < users.length; i++) {
                            dutyUserGridStore.add({
                                WH_NO: users[i].WH_NO,
                                WH_USERID: users[i].WH_USERID,
                                WH_USERNAME: users[i].WH_USERNAME,
                            });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // ------ 主畫面 ------
    var T1Store = viewModel.getStore('BC_WHPICKDOC');
    function T1Load() {

        T2Store.removeAll();

        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue().split(" ")[0]);
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Store.getProxy().setExtraParam("p3", assignType);
        T1Tool.moveFirst();
        
        if (assignType == "=") {
            T1Grid.getColumns()[0].show();
        } else {
            T1Grid.getColumns()[0].hide();
        }
    }
    var T2Store = viewModel.getStore('BC_WHPICK');
    function T2Load() {
        if (T1LastRec != null) {
            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["WH_NO"]);
            T2Store.getProxy().setExtraParam("p1", T1LastRec.data["PICK_DATE"]);
            T2Store.getProxy().setExtraParam("p2", T1LastRec.data["DOCNO"]);
            T2Tool.moveFirst();
        }
    }

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    width:'100%',
                    items: [
                        {
                            xtype: 'combo',
                            store: whnoQueryStore,
                            name: 'P0',
                            id: 'P0',
                            labelWidth: mLabelWidth,
                            width: mWidth,
                            fieldLabel: '庫房',
                            displayField: 'WH_NAME',
                            valueField: 'WH_NO',
                            queryMode: 'local',
                            fieldCls: 'required',
                            anyMatch: true,
                            allowBlank: false,
                            typeAhead: true,
                            forceSelection: true,
                            triggerAction: 'all',
                            multiSelect: false,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
                            listeners: {
                                select: function (oldValue, newValue, eOpts) {
                                    getDutyUserCombo(newValue.data.WH_NO);
                                }
                            }
                        },
                        //{
                        //    xtype: 'displayfield',
                        //    name: 'P0',
                        //    id: 'P0',
                        //        labelWidth: mLabelWidth,
                        //    width: mWidth,
                        //    fieldLabel: '庫房',
                        //    labelAlign: 'right',
                        //    value: "560000 中央庫房"
                        //},
                        {
                            xtype: 'datefield',
                            fieldLabel: '申請日期',
                            name: 'P4',
                            id: 'P4',
                            fieldCls: 'required',
                            labelWidth: mLabelWidth,
                            width: 150,
                            allowBlank: false,
                            padding: '0 4 0 4',
                            //maxValue: getMaxDate(),
                            //minValue: new Date(),
                            value: new Date(),
                        },
                        {
                            xtype: 'datefield',
                            fieldLabel: '揀貨日期',
                            name: 'P1',
                            id: 'P1',
                            fieldCls: 'required',
                            labelWidth: mLabelWidth,
                            width: 150,
                            allowBlank: false,
                            padding: '0 4 0 4',
                            maxValue: getMaxDate(),
                            minValue: new Date(),
                            value: new Date()
                        },

                        //{
                        //    xtype: 'radiogroup',
                        //    name: 'P2',
                        //    fieldLabel: '申請類別',
                        //    items: [
                        //        { boxLabel: '臨時', name: 'P2', inputValue: '2', width: 70, checked: true },
                        //        { boxLabel: '常態', name: 'P2', inputValue: '1' }
                        //    ],
                        //    listeners: {
                        //        change: function (field, newValue, oldValue) {
                        //            changeApplyKind(newValue['P2']);
                        //        }
                        //    },
                        //    labelWidth: 100,
                        //    width: 250
                        //},
                        {
                            xtype: 'radiogroup',
                            name: 'P3',
                            fieldLabel: '分配狀態',
                            items: [
                                { boxLabel: '待分配', name: 'P3', inputValue: '=', width: 70, checked: true },
                                { boxLabel: '已分配', name: 'P3', inputValue: '<>' }
                            ],
                            listeners: {
                                change: function (field, newValue, oldValue) {
                                    changeAssignType(newValue['P3']);
                                }
                            },
                            labelWidth: 100,
                            width: 250
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');

                                if (!T1Query.getForm().findField('P0').getValue() ||
                                    !T1Query.getForm().findField('P1').getValue()) {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房</span>與<span style=\'color:red\'>揀貨日期</span>為必填');
                                    return;
                                }

                                T1Load();

                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.findField('P0').setValue('');
                                f.findField('P0').clearInvalid();
                                f.findField('P1').setValue();
                                f.findField('P1').clearInvalid();
                                f.findField('P3').setValue('=');
                                //f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        },
                        {
                            xtype: 'component',
                            flex: 1
                        },
                        {
                            xtype: 'container',
                            layout: 'hbox',
                            right: '100%',
                            items: [
                                {
                                    xtype: 'button',
                                    text: '揀貨',
                                    //cls:'pull-right',
                                    
                                    //floated:true,
                                    handler: function () {
                                        parent.link2('/Form/Mobile/CD0004', ' 點選申請單編號(PDA)(CD0004)', true);
                                    }
                                },
                                {
                                    xtype: 'button',
                                    text: '出庫',
                                    //cls:'pull-right',

                                    //floated:true,
                                    handler: function () {
                                        parent.link2('/Form/Mobile/CD0008', ' 裝箱出庫作業(CD0008)', true);
                                    }
                                },
                                {
                                    xtype: 'button',
                                    text: '資料查詢',
                                    //cls:'pull-right',

                                    //floated:true,
                                    handler: function () {
                                        parent.link2('/Form/Index/CD0010', ' 揀貨資料查詢(PDA)(CD0010)', true);
                                    }
                                }
                            ]
                        }
                    ]
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            //{
            //    text: '查詢新申請單資料',
            //    id: 'newApply',
            //    name: 'newApply',
            //    handler: function () {

            //        if (!T1Query.getForm().findField('P0').getValue() ||
            //            !T1Query.getForm().findField('P1').getValue()) {
            //            Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房</span>與<span style=\'color:red\'>核撥日期</span>為必填');
            //            return;
            //        }

            //        T31Grid.setHeight(windowHeight / 2 - 30);
            //        T32Grid.setHeight(windowHeight / 2 - 30); 

            //        T31LastRec = null;
            //        T31Store.removeAll();
            //        T31Load("true");
            //        T32Store.removeAll();

            //        //var myMask = new Ext.LoadMask(viewport, {msg:''});
            //        //myMask.show();

            //        newApplyWindow.show();

                    
            //    }
            //},
            {
                text: '列印申請單',
                id: 'print',
                name: 'print',
                handler: function () {
                    
                    var selection = T1Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請點選要列印的申請單資料');
                        return;
                    }

                    var docnos = '';
                    
                    for (var i = 0; i < selection.length; i++) {
                        if (docnos != '') {
                            docnos += '|';
                        }
                        docnos += selection[i].data.DOCNO;
                    }
                    
                    showReportNoUser(docnos)
                    
                }
            },
            {
                text: '分配揀貨員',
                id: 'assignUser',
                name: 'assignUser',
                handler: function () {
                    if (!T1Query.getForm().findField('P0').getValue() ||
                        !T1Query.getForm().findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房</span>與<span style=\'color:red\'>揀貨日期</span>為必填');
                        return;
                    }

                    var selection = T1Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請點選要分配揀貨員的申請單資料');
                        return;
                    }

                    T51Store.removeAll();

                    T51LastRec = null;

                    T51Query.getForm().findField('T51P0').setValue(T1Query.getForm().findField('P0').getValue());
                    T51Query.getForm().findField('T51P1').setValue(T1Query.getForm().findField('P1').rawValue);
                    T51Query.getForm().findField('T51P2').setValue('');

                    //myMask.show();
                    distributionWindow.show();
                }
            },
            {
                text: '已排揀貨批次',
                id: 'assignedDocs',
                name: 'assignedDocs',
                handler: function () {
                    if (!T1Query.getForm().findField('P0').getValue() ||
                        !T1Query.getForm().findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房</span>與<span style=\'color:red\'>揀貨日期</span>為必填');
                        return;
                    }

                    T61Grid.setHeight(windowHeight / 2 - 60);
                    T62Grid.setHeight(windowHeight / 2 - 45); 

                    T61LastRec = null;
                    T62Store.removeAll();
                    T61Load();

                    //myMask.show();
                    distritedListnWindow.show();
                }
            },
            {
                text: '全部重新分配',
                id: 'clearAssign',
                name: 'clearAssign',
                handler: function () {

                    msglabel('訊息區:');

                    if (!T1Query.getForm().findField('P0').getValue() || 
                        !T1Query.getForm().findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房</span>與<span style=\'color:red\'>揀貨日期</span>為必填');
                        return;
                    }

                    clearAssign();
                }
            }
        ]
    });
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
                text: "申請單號碼",
                dataIndex: 'DOCNO',
                width: 155
            },
            {
                text: "申請部門",
                dataIndex: 'APPDEPT_NAME',
                width: 110
            },
            {
                text: "申請品項數",
                dataIndex: 'APPITEM_SUM',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "申請總數量",
                dataIndex: 'APPQTY_SUM',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "揀貨批次",
                dataIndex: 'LOT_NO',
                width: 120
            },
            {
                text: "申請單備註",
                dataIndex: 'APPLY_NOTE',
                width: 120
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                T1LastRec = record;
                T2Load();
            }
        }
    });
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        border: false,
        plain: true,
        displayInfo: true,
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: [
            //{
            //    xtype: 'rownumberer'
            //},
            {
                text: "項次",
                dataIndex: 'SEQ',
                width: 60,
                align: 'right',
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100,
                //renderer: function (val, meta, record) {

                //    var mmcode = record.data.MMCODE;
                //    var param3 = record.data.DOCNO;
                //    return '<a href=javascript:void(0)>' + mmcode + '</a>';
                //},
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200
            },
            {
                text: "核撥量",
                dataIndex: 'APPQTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "最小撥補單位",
                dataIndex: 'BASE_UNIT',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                width: 100,
            },
            {
                text: "備註",
                dataIndex: 'APLYITEM_NOTE',
                width: 150
            },
            {
                header: "",
                flex: 1
            }
        ]
    });

    function getMaxDate() {
        var today = new Date();
        today.setDate(today.getDate() + 1);

        return today
    }

    function changeAssignType(type) {
        assignType = type;

        if (assignType == "=") {  //未分配
            Ext.getCmp('assignUser').enable();
            Ext.getCmp('assignedDocs').disable();
            Ext.getCmp('clearAssign').disable();
        } else {
            Ext.getCmp('assignUser').disable();
            Ext.getCmp('assignedDocs').enable();
            Ext.getCmp('clearAssign').enable();
        }
    }
    // --------------------

    // ------ 查詢新申請單資料 ------
    var T31Store = viewModel.getStore('ME_DOCM');
    function T31Load(runDelete) {
        if (T1Query.getForm().findField('P0').getValue() &&
            T1Query.getForm().findField('P1').getValue()) {

            T31Query.getForm().findField('T31P0').setValue(T1Query.getForm().findField('P0').getValue());
            T31Query.getForm().findField('T31P1').setValue(T1Query.getForm().findField('P1').rawValue);

            T31Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue().split(' ')[0]);
            T31Store.getProxy().setExtraParam("runDelete", runDelete);
            T31Tool.moveFirst();
        }
    }
    var T32Store = viewModel.getStore('ME_DOCD');
    function T32Load() {
        if (T31LastRec != null) {
            T32Store.getProxy().setExtraParam("p0", T31LastRec.data["DOCNO"]);
            T32Tool.moveFirst();
        }
    }
    var T31Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
                            width: 200,
                            fieldLabel: '庫房',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '揀貨日期',
                            name: 'T31P1',
                            id: 'T31P1',
                            labelWidth: 60,
                            width: 150,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'button',
                            text: '全部轉入揀貨',
                            handler: function () {

                                Ext.MessageBox.confirm('全部轉入揀貨', '確定要轉入全部申請單資料嗎?', function (btn, text) {
                                    if (btn === 'yes') {
                                        transferToBcwhpick("");
                                    }
                                }
                                );
                            }
                        },
                        {
                            xtype: 'button',
                            text: '取消',
                            handler: function () {
                                newApplyWindow.hide();
                                T1Load();
                                //myMask.hide();
                            }
                        }
                    ]
                }
            ]
        }]
    });
    var T31Tool = Ext.create('Ext.PagingToolbar', {
        store: T31Store,
        border: false,
        plain: true,
        displayInfo: true
    });
    var T31Grid = Ext.create('Ext.grid.Panel', {
        store: T31Store,
        //plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight / 2 - 30,
        dockedItems: [
            //{
            //    dock: 'top',
            //    xtype: 'toolbar',
            //    items: [T31Query]
            //},
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T31Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "申請單號碼",
                dataIndex: 'DOCNO',
                width: 155
            },
            {
                text: "申請部門代碼",
                dataIndex: 'APPDEPT_NAME',
                width: 120
            },
            {
                text: "申請單分類",
                dataIndex: 'APPLY_KIND_N',
                width: 100
            },
            {
                text: "申請品項數",
                dataIndex: 'APPCNT',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "申請總數量",
                dataIndex: 'APPQTY_SUM',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                flex: 1,
                renderer: function (val, meta, record) {

                    var mmcode = record.data.MMCODE;
                    var param3 = record.data.DOCNO;
                    return '<a href=javascript:void(0)>單筆轉入揀貨</a>';
                },
                width: 120
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {

                T31LastRec = record;
                if (T31cell == '') {
                    T32Load();
                }
                T31cell = '';
            },
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {

                if (cellIndex != 6) {
                    return;
                }
                T31LastRec = record;
                T31cell = 'cellClick';

                Ext.MessageBox.confirm('單筆轉入揀貨', '確定要轉入此申請單資料嗎?', function (btn, text) {
                    if (btn === 'yes') {
                        transferToBcwhpick(T31LastRec.data.DOCNO);
                    }
                }
                );
            },
        }
    });
    var T32Tool = Ext.create('Ext.PagingToolbar', {
        store: T32Store,
        border: false,
        plain: true,
        displayInfo: true
    });
    var T32Grid = Ext.create('Ext.grid.Panel', {
        store: T32Store,
        //plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight / 2 - 35,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T32Tool]
            }
        ],
        columns: [
            //{
            //    xtype: 'rownumberer'
            //},
            {
                text: "項次",
                dataIndex: 'SEQ',
                width: 50
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 110
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "核撥量",
                dataIndex: 'APPQTY',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "最小撥補單位",
                dataIndex: 'BASE_UNIT',
                width: 90
            }
        ]
    });

    function transferToBcwhpick(docno) {

        var wh_no = T1Query.getForm().findField('P0').getValue().split(' ')[0];
        var pick_date = T1Query.getForm().findField('P1').getValue();
        var apply_date = T1Query.getForm().findField('P4').getValue();

        Ext.Ajax.request({
            url: TransferTobcwhpickUrl,
            method: reqVal_p,
            params: {
                WH_NO: wh_no,
                PICK_DATE: pick_date,
                APPLY_DATE: apply_date,
                DOCNO: docno
            },
            success: function (response) {
                //newApplyWindow.hide();
                T1Load();
                T31Load("false");
                msglabel("轉入揀貨成功");
                T32Store.removeAll();
                //var myMask = new Ext.LoadMask(viewport, { msg: '' });
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    var newApplyWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal:true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [
                    T31Query,
                    {
                    xtype: 'panel',
                    itemId: 't31Grid',
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    title: '',
                    border: false,
                    height: '50%',
                    minHeight: windowHeight / 2 - 30,
                    items: [T31Grid]
                },
                {
                    xtype: 'splitter',
                    collapseTarget: 'dev'
                }, {
                    xtype: 'panel',
                    autoScroll: true,
                    itemId: 't32Grid',
                    region: 'south',
                    layout: 'fit',
                    //collapsible: true,
                    title: '明細',
                    //titleCollapse: true,
                    border: false,
                    height: '50%',
                    split: true,
                    items: [T32Grid]
                }
                ],
            }
        ],
        //items: [T31Grid, T32Grid],
        width: "700px",
        height: windowHeight,
        resizable: false,
        closable: false,
        //x: ($(window).width() / 2) - 200,
        y: 0, 
        title: "新申請單資料",
        buttons: [{
            text: '關閉',
            handler: function () {
                //var myMask = new Ext.LoadMask(viewport, { msg: '' });
                //myMask.hide();

                this.up('window').hide();
                T1Load();
            }
        }],
        
    });
    newApplyWindow.hide();
    // --------------------

    // ------ 分配揀貨員 ------
    var T51Store = viewModel.getStore('DistributeRegular');
    function T51Load() {
        T51Store.removeAll();

        if (T1Query.getForm().findField('P0').getValue() &&
            T1Query.getForm().findField('P1').getValue()) {

            T51Query.getForm().findField('T51P0').setValue(T1Query.getForm().findField('P0').getValue());
            T51Query.getForm().findField('T51P1').setValue(T1Query.getForm().findField('P1').rawValue);

            T51Store.getProxy().setExtraParam("WH_NO", T1Query.getForm().findField('P0').getValue().split(' ')[0]);
            T51Store.getProxy().setExtraParam("PICK_DATE", T1Query.getForm().findField('P1').getValue());
            T51Store.getProxy().setExtraParam("USER_COUNT", T51Query.getForm().findField('T51P2').getValue());
            T51Store.getProxy().setExtraParam("SORTER", T51Query.getForm().findField('T51P3').getValue()['T51P3']);

            var selection = T1Grid.getSelection();
            var docnos = getSelecionDocnos(selection);
            T51Store.getProxy().setExtraParam("DOCNOS", docnos);

            T51Tool.moveFirst();
        }
    }
    var T52Store = viewModel.getStore('GetTemplotdocseqDetail');
    function T52Load() {
        T52Store.removeAll();

        T52Store.getProxy().setExtraParam("WH_NO", T51LastRec.data["WH_NO"]);
        T52Store.getProxy().setExtraParam("CALC_TIME", T51LastRec.data["CALC_TIME"]);
        T52Store.getProxy().setExtraParam("GROUP_NO", T51LastRec.data["GROUP_NO"]);
        T52Tool.moveFirst();
    }
    var T51Query = Ext.widget({
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
                    id: 'PanelP51',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            name: 'T51P0',
                            id: 'T51P0',
                            labelWidth: 60,
                            width: 120,
                            fieldLabel: '庫房',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '揀貨日期',
                            name: 'T51P1',
                            id: 'T51P1',
                            labelWidth: 60,
                            width: 120,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '分配人數',
                            name: 'T51P2',
                            id: 'T51P2',
                            labelWidth: 60,
                            width: 140,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                            fieldCls: 'required',
                            enforceMaxLength: false,
                            maxLength: 14,
                            hideTrigger: true
                        },
                        //{
                        //    xtype: 'radiogroup',
                        //    name: 'T51P3',
                        //    fieldLabel: '排序方式',
                        //    items: [
                        //        { boxLabel: '申請單號項次', name: 'T51P3', inputValue: 'docno,seq', width: 90, checked: true },
                        //        { boxLabel: '英文品名', name: 'T51P3', inputValue: 'mmname_e', width: 70 },
                        //        { boxLabel: '儲位', name: 'T51P3', inputValue: 'store_loc', }
                        //    ],
                        //    //listeners: {
                        //    //    change: function (field, newValue, oldValue) {
                        //    //        //changeApplyKind(newValue['P2']);
                        //    //        if (T51LastRec != null) {
                        //    //            T52Load();
                        //    //        }
                        //    //    }
                        //    //},
                        //    labelWidth: 60,
                        //    //width: 250
                        //},
                        {
                            xtype: 'button',
                            text: '計算分配',
                            handler: function () {
                                if (!T51Query.getForm().findField('T51P2').getValue()) {
                                    Ext.Msg.alert('提醒', '請輸入<span style=\'color:red\'>要分配的人數</span>');
                                    return;
                                }

                                T51Load();

                            }
                        },
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP52',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'radiogroup',
                            name: 'T51P3',
                            fieldLabel: '排序方式',
                            items: [
                                { boxLabel: '申請單號項次', name: 'T51P3', inputValue: 'docno,seq', width: 90 },
                                { boxLabel: '英文品名', name: 'T51P3', inputValue: 'mmname_e', width: 70 },
                                { boxLabel: '儲位', name: 'T51P3', inputValue: 'substr(store_loc,3,18)', width: 70, checked: true }
                            ],
                            labelAlign: 'right',
                            labelWidth: 60,
                            //width: 250
                        }
                    ]
                }
            ]
        }]
    });
    var T51Tool = Ext.create('Ext.PagingToolbar', {
        store: T51Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '儲存資料',
                id: 'btnDistributeUpdate',
                name: 'btnDistributeUpdate',
                handler: function () {
                    
                    var tempData = T51Grid.getStore().data.items;
                    var data = [];
                    if (tempData.length > 0)
                    {
                        for (var i = 0; i < tempData.length; i++) {
                            tempData[i].data.PICK_DATE = T1Query.getForm().findField('P1').getValue();
                            if (!tempData[i].data.PICK_USERID) {
                                Ext.Msg.alert('提醒', '所有批次都需有負責人');
                                return;
                            }
                        }
                        for (var i = 0; i < tempData.length; i++) {
                            for (var j = 0; j < tempData.length; j++) {
                                if (i != j) {
                                    if (tempData[i].data.PICK_USERID == tempData[j].data.PICK_USERID) {
                                        Ext.Msg.alert('提醒', '負責人不可重複');
                                        return;
                                    }
                                }
                            }
                        }
                        for (var i = 0; i < tempData.length; i++) {
                            for (var j = 0; j < tempData[i].data.Details.length; j++) {
                                var item = {
                                    WH_NO: tempData[i].data.WH_NO,
                                    PICK_DATE: tempData[i].data.PICK_DATE,
                                    LOT_NO: tempData[i].data.LOT_NO,
                                    PICK_USERID: tempData[i].data.PICK_USERID,
                                    CALC_TIME: tempData[i].data.CALC_TIME,
                                    DOCNO: tempData[i].data.Details[j].DOCNO,
                                    SEQ: tempData[i].data.Details[j].SEQ,
                                    PICK_SEQ: tempData[i].data.Details[j].PICK_SEQ,
                                }
                                data.push(item);
                            }
                        }
                        Ext.Ajax.request({
                            url: '/api/CD0009/SetPickUser',
                            method: reqVal_p,
                            params: { ITEM_STRING: Ext.util.JSON.encode(data) },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    msglabel("儲存成功");
                                    distributionWindow.hide();

                                    //T1Query.getForm().findField('P3').setValue('<>');
                                    T1Load();

                                    //myMask.hide();
                                    distributionWindow.hide();
                                    msglabel("揀貨員分配成功");
                                }
                            },
                            failure: function (response, options) {

                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '沒有可分配的群組');
                }
            }]
    });
    var T51Grid = Ext.create('Ext.grid.Panel', {
        store: T51Store,
        //plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        draggle: false,
        height: windowHeight / 2 - 30,
        dockedItems: [
            //{
            //    dock: 'top',
            //    xtype: 'toolbar',
            //    items: [T51Query]
            //},
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T51Tool]
            }
        ],
        columns: [
            {
                text: "群組",
                dataIndex: 'GROUP_NO',
                width: 90
            },
            {
                text: "品項數",
                dataIndex: 'APPITEM_SUM',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "申請數量合計",
                dataIndex: 'APPQTY_SUM',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "<span style='color:red'>分配揀貨人員</span>",
                dataIndex: 'PICK_USERID',
                width: 180,
                renderer: function (value) {

                    for (var i = 0; i < dutyUserGridStore.data.items.length; i++) {
                        if (dutyUserGridStore.data.items[i].data.WH_USERID == value) {
                            return dutyUserGridStore.data.items[i].data.WH_USERNAME;
                        }
                    }
                },
                editor: {
                    xtype: 'combobox',
                    store: dutyUserGridStore,
                    displayField: 'WH_USERNAME',
                    valueField: 'WH_USERID',
                    queryMode: 'local'
                }
            }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ],
        listeners: {
            afterernder: function () {

                var store = this.getStore();
            },
            itemclick: function (self, record, item, index, e, eOpts) {
                T51LastRec = record;

                if (T51cell != 'cell') {
                    T52Load();
                }

                T51cell = '';
            },
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                
                if (cellIndex != 3) {
                    return;
                }
                // T61LastRec = record;
                T51cell = 'cell';
            },
        }
    });
    var T52Tool = Ext.create('Ext.PagingToolbar', {
        store: T52Store,
        border: false,
        plain: true,
        displayInfo: true,
    });
    var T52Grid = Ext.create('Ext.grid.Panel', {
        store: T52Store,
        //plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        maxHeight: windowHeight / 2 - 35,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T52Tool]
            }
        ],
        columns: [
            {
                text: "申請單號碼",
                dataIndex: 'DOCNO',
                width: 155
            },
            {
                text: "項次",
                dataIndex: 'SEQ',
                width: 60,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 120
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 150,
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 150,
            },
            {
                text: "申請量",
                dataIndex: 'APPQTY',
                width: 80,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "撥補單位",
                dataIndex: 'BASE_UNIT',
                width: 80,
            },
            {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                width: 80,
            }
        ]
    });
    var distributionWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [
                    T51Query,
                    {
                    xtype: 'panel',
                    itemId: 't51Grid',
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    title: '',
                    border: false,
                    height: '50%',
                    minHeight: windowHeight / 2 - 30,
                    items: [T51Grid]
                },
                //{
                //    xtype: 'splitter',
                //    collapseTarget: 'dev'
                //    },
                    {
                    xtype: 'panel',
                    autoScroll: true,
                    itemId: 't52Grid',
                    region: 'south',
                    layout: 'fit',
                    //collapsible: true,
                    //title: '明細',
                    //titleCollapse: true,
                    border: false,
                    height: '50%',
                    split: true,
                    items: [T52Grid]
                }
                ],
            }
        ],

        //items: [T51Grid, T52Grid],
        width: "900px",
        height: windowHeight,
        resizable: false,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0, 
        title: "分配揀貨員",
        //listeners: {
        //    close: function (panel, eOpts) {
        //        myMask.hide();
        //    }
        //}
        buttons: [{
            text: '關閉',
            handler: function () {
                Ext.MessageBox.confirm('', '是否取消分配揀貨員?', function (btn, text) {
                    if (btn === 'yes') {
                        distributionWindow.hide();
                        //myMask.hide();
                    }
                });
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                
                distributionWindow.setY(0);
            }
        }
    });
    distributionWindow.hide();
    function getSelecionDocnos(selection) {
        var docnos = '';
        for (var i = 0; i < selection.length; i++) {
            docnos = docnos + "'" + selection[i].data.DOCNO + "'";
            if (i < selection.length - 1) {
                docnos += ",";
            }
        }
        return docnos;
    }
    // --------------------

    // ------ 已排揀貨批次 ------
    var T61Store = viewModel.getStore('DistributedMaster');
    function T61Load(runDelete) {
        T61Store.removeAll();
        T62Store.removeAll();
        if (T1Query.getForm().findField('P0').getValue() &&
            T1Query.getForm().findField('P1').getValue()) {
            
            T61Query.getForm().findField('T61P0').setValue(T1Query.getForm().findField('P0').getValue());
            T61Query.getForm().findField('T61P1').setValue(T1Query.getForm().findField('P1').rawValue);

            T61Store.getProxy().setExtraParam("WH_NO", T1Query.getForm().findField('P0').getValue().split(' ')[0]);
            T61Store.getProxy().setExtraParam("PICK_DATE", T1Query.getForm().findField('P1').getValue());
            
            T61Tool.moveFirst();
        }
    }
    var T61Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        items: [{
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP61',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            name: 'T61P0',
                            id: 'T61P0',
                            labelWidth: 60,
                            width: 150,
                            fieldLabel: '庫房',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '揀貨日期',
                            name: 'T61P1',
                            id: 'T61P1',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        }
                    ]
                },
            ]
        }]
    });
    var T61Tool = Ext.create('Ext.PagingToolbar', {
        store: T61Store,
        border: false,
        plain: true,
        displayInfo: true,
    });
    var T61Grid = Ext.create('Ext.grid.Panel', {
        store: T61Store,
        //plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight / 2 - 60,
        dockedItems: [
            //{
            //    dock: 'top',
            //    xtype: 'toolbar',
            //    items: [T61Query]
            //},
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T61Tool]
            }
        ],
        columns: [
            {
                text: "揀貨批次",
                dataIndex: 'LOT_NO',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "品項數",
                dataIndex: 'APPITEM_SUM',
                width: 120,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "件數合計",
                dataIndex: 'APPQTY_SUM',
                width: 120,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "",
                renderer: function (val, meta, record) {
                    return '<a href=javascript:void(0)>列印揀貨單</a>';
                },
                width: 80
            },
            {
                text: "",
                renderer: function (val, meta, record) {
                    return '<a href=javascript:void(0)>取消此批揀貨</a>';
                },
                width: 90
            },
            {
                flex: 1,
            },
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {

                T61LastRec = record;
                if (T61cell == '') {
                    T62Load();
                }
                T61cell = '';
            },
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                
                if (cellIndex != 3 && cellIndex != 4) {
                    return;
                }
                // T61LastRec = record;
                T61cell = 'cell';

                if (cellIndex == 4) {
                    // CancelDistributed
                    Ext.Ajax.request({
                        url: '/api/CD0009/CancelDistributed',
                        method: reqVal_p,
                        params: {
                            WH_NO: record.data.WH_NO,
                            PICK_DATE: record.data.PICK_DATE,
                            LOT_NO: record.data.LOT_NO
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel("取消此批揀貨成功");
                                T61Load();
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                } else {
                    showReport(record);
                }

                //T62Load();
            },
        }
    });
    var T62Store = viewModel.getStore('DistributedDetail');
    function T62Load() {
        if (T61LastRec != null) {
            T62Store.getProxy().setExtraParam("WH_NO", T1Query.getForm().findField('P0').getValue().split(' ')[0]);
            T62Store.getProxy().setExtraParam("PICK_DATE", T1Query.getForm().findField('P1').getValue());
            T62Store.getProxy().setExtraParam("LOT_NO", T61LastRec.data["LOT_NO"]);
            T62Tool.moveFirst();
        }
    }
    var T62Tool = Ext.create('Ext.PagingToolbar', {
        store: T62Store,
        border: false,
        plain: true,
        displayInfo: true
    });
    var T62Grid = Ext.create('Ext.grid.Panel', {
        store: T62Store,
        //plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight / 2 - 45,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T62Tool]
            }
        ],
        columns: [
            //{
            //    xtype: 'rownumberer'
            //},
            {
                text: "申請單號碼",
                dataIndex: 'DOCNO',
                width: 155
            },
            {
                text: "項次",
                dataIndex: 'SEQ',
                width: 50
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 110
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "申請量",
                dataIndex: 'APPQTY',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "撥補單位",
                dataIndex: 'BASE_UNIT',
                width: 90
            },
            {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                width: 90
            },
            {
                text: "揀貨人員",
                dataIndex: 'PICK_USERNAME',
                width: 120
            },
            {
                text: "備註",
                dataIndex: 'APLYITEM_NOTE',
                width: 120
            }
        ]
    });
    var distritedListnWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [
                    T61Query,
                    {
                    xtype: 'panel',
                    itemId: 't61Grid',
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    title: '',
                    border: false,
                    height: '50%',
                    minHeight: windowHeight / 2 - 60,
                    items: [T61Grid]
                },
                {
                    xtype: 'splitter',
                    collapseTarget: 'dev'
                }, {
                    xtype: 'panel',
                    autoScroll: true,
                    itemId: 't62Grid',
                    region: 'south',
                    layout: 'fit',
                    //collapsible: true,
                    //title: '明細',
                    //titleCollapse: true,
                    border: false,
                    height: '50%',
                    split: true,
                    items: [T62Grid]
                }
                ],
            }
        ],
        //items: [T61Grid, T62Grid],
        width: 910,
        height: windowHeight,
        resizable: false,
        closable: false,
        //x: (windowWidth / 2),
        y: 0, 
        title: "已排揀貨批次",
        buttons: [{
            text: '關閉',
            handler: function () {
                //myMask.hide();
                this.up('window').hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                
                distritedListnWindow.setY(0);
            }
        }
    });
    distritedListnWindow.hide();

    function showReport(record) {
        if (!win) {
            
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?WH_NO=' + T1Query.getForm().findField('P0').getValue()
                + '&PICK_DATE=' + dateTransform(T1Query.getForm().findField('P1').getValue())
                + '&LOT_NO=' + record.data.LOT_NO
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
    function showReportNoUser(docnos) {
        if (!win) {

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportNoUserUrl + '?WH_NO=' + T1Query.getForm().findField('P0').getValue()
                + '&PICK_DATE=' + dateTransform(T1Query.getForm().findField('P1').getValue())
                + '&DOCNOS=' + docnos
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
    function dateTransform(date) {
        var yyyy = date.getFullYear().toString();
        var m = (date.getMonth() + 1).toString();
        var d = date.getDate().toString();
        var mm = m.length < 2 ? "0" + m : m;
        var dd = d.length < 2 ? "0" + d : d;
        return yyyy + "-" + mm + "-" + dd;
    }
    // --------------------

    // ------ 全部重新分配 ------
    function clearAssign() {
        var msg = "是否確定要刪除所有已分配揀貨資料?";
        Ext.MessageBox.confirm('全部重新分配', msg, function (btn, text) {
            if (btn === 'yes') {
                
                var wh_no = T1Query.getForm().findField('P0').getValue();
                var pick_date = T1Query.getForm().findField('P1').getValue();

                Ext.Ajax.request({
                    url: '/api/CD0009/ClearDistribution',
                    method: reqVal_p,
                    params: {
                        WH_NO: wh_no,
                        PICK_DATE: pick_date
                    },
                    success: function (response) {
                        msglabel("全部重新分配成功")
                        T1Load();
                    },
                    failure: function (response, options) {
                        Ext.Msg.alert('失敗', '發生例外錯誤');
                    }
                });

            }
        }
        );
    }

    // --------------------

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
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '50%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    },
                    //{
                    //    itemId: 't3Grid',
                    //    region: 'south',
                    //    layout: 'fit',
                    //    collapsible: false,
                    //    title: '',
                    //    height: '34%',
                    //    split: true,
                    //    items: [T3Grid]
                    //}
                ]
            }
        ]
    });


    var myMask = new Ext.LoadMask(viewport, { msg: '' });
    //myMask.hide();

    changeAssignType('=');

    Ext.on('resize', function () {
        windowWidth = $(window).width();
        windowHeight = $(window).height();
        newApplyWindow.setHeight(windowHeight);
        T31Grid.setHeight(windowHeight / 2 - 30);
        T32Grid.setHeight(windowHeight / 2 - 30);

        distributionWindow.setHeight(windowHeight);
        T51Grid.setHeight(windowHeight / 2 - 30);
        T52Grid.setHeight(windowHeight / 2 - 30);

        distritedListnWindow.setHeight(windowHeight);
        T61Grid.setHeight(windowHeight / 2 - 60);
        T62Grid.setHeight(windowHeight / 2 - 45);

    });
});