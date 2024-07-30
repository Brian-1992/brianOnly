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
    var TransferTobcwhpickUrl = '/api/CD0002/TransferToBcwhpick';
    var dutyUpdateUrl = '/api/CD0002/UpdateDuty';
    var complexityUpdateUrl = '/api/CD0002/UpdateComplexityByInid';

    var newApplyGet = '/api/CD0002/GetNewApply';
    var reportUrl = '/Report/C/CD0002.aspx';

    var windowWidth = $(window).width();
    var windowHeight = $(window).height();

    var T1LastRec = null;
    var T31LastRec = null;
    var T41LastRec = null;
    var T61LastRec = null;
    var T1Name = '分配揀貨人員';

    var wh_kind = '';       // 1:衛材庫人員 2:藥庫人員
    var apply_kind = '';    // 1:常態申請 2:臨時申請 
    var isDistributed = ''; // =:已分配 <>:待分配
    var T31cell = '';
    var T41cell = '';
    var T61cell = '';
    var max_temp_lot_no = '';
    var dutyUser = '';
    var dutyUserName = '';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    //var userId, userName, userInid, userInidName;

    var viewModel = Ext.create('WEBAPP.store.CD0002VM');

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
                        T1Query.getForm().findField('P0').setValue(wh_nos[0].WH_NO);
                        getDutyUserCombo(wh_nos[0].WH_NO);
                        if (apply_kind == "2") {
                            T1Query.getForm().findField('P4').enable();
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getWhnoCombo();

    // 值日生清單
    var dutyUserQueryStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_USERID', 'WH_USER_NAME', 'IS_DUTY']
    });
    // 分派人員清單
    var dutyUserGridStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_USERID', 'WH_USER_NAME']
    });
    function getDutyUserCombo(wh_no) {
        dutyUser = '';
        T1Query.getForm().findField('P4').clearValue();
        Ext.getCmp('btnDuty').disable();
        dutyUserQueryStore.removeAll();
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
                            dutyUserQueryStore.add({
                                WH_NO: users[i].WH_NO,
                                WH_USERID: users[i].WH_USERID,
                                WH_USERNAME: users[i].WH_USERNAME,
                                IS_DUTY: users[i].IS_DUTY,
                            });
                            dutyUserGridStore.add({
                                WH_NO: users[i].WH_NO,
                                WH_USERID: users[i].WH_USERID,
                                WH_USERNAME: users[i].WH_USERNAME,
                            });

                            if (users[i].IS_DUTY == 'Y') {
                                dutyUser = users[i].WH_USERID;
                                dutyUserName = users[i].WH_USERNAME;
                                T1Query.getForm().findField('P4').setValue(users[i].WH_USERID);
                                Ext.getCmp('btnDuty').setDisabled(false);
                            }
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // #region ------ 主畫面 ------
    var T1Store = viewModel.getStore('BC_WHPICKDOC');
    function T1Load(resetComplexity) {
        //myMask.hide();
        T2Store.removeAll();

        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue().split(" ")[0]);
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", apply_kind);
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("resetComplexity", resetComplexity);
        T1Tool.moveFirst();
    }
    T1Store.on('load', function (store, options) {
        var columns = T1Grid.getColumns();
        var index = getColumnIndex(columns, 'COMPLEXITY');

        if (apply_kind == '2' || apply_kind == '3') {     //臨時
            if (index >= 0) {
                T1Grid.getColumns()[index].hide();
            }
            //Ext.getCmp('assignUser').disable();
        } else {
            if (index >= 0) {
                T1Grid.getColumns()[index].show();
            }
        }
    });
    var T2Store = viewModel.getStore('BC_WHPICK');
    function T2Load() {
        if (T1LastRec != null) {
            T2Store.getProxy().setExtraParam("wh_no", T1LastRec.data["WH_NO"]);
            T2Store.getProxy().setExtraParam("pick_date", T1LastRec.data["PICK_DATE"]);
            T2Store.getProxy().setExtraParam("docnos", T1LastRec.data["DOCNOS"]);
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
                    items: [
                        {
                            xtype: 'combo',
                            store: whnoQueryStore,
                            name: 'P0',
                            id: 'P0',
                            labelWidth: mLabelWidth,
                            width: 260,
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

                                    wh_kind = newValue.data.WH_KIND;
                                    getDutyUserCombo(newValue.data.WH_NO);
                                    if (apply_kind == "2") {
                                        T1Query.getForm().findField('P4').enable();
                                    }
                                    //T1Query.getForm().findField('P4').enable();
                                    //Ext.getCmp('btnDuty').setDisabled(false);
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
                            value: getMaxDate()
                        },
                        {
                            xtype: 'radiogroup',
                            name: 'P2',
                            fieldLabel: '申請類別',
                            items: [
                                { boxLabel: '臨時', name: 'P2', inputValue: '2', width: 70, checked: true },
                                { boxLabel: '常態', name: 'P2', inputValue: '1', width: 70, },
                                { boxLabel: '被服與資訊耗材', name: 'P2', inputValue: '3', width: 120, },
                            ],
                            listeners: {
                                change: function (field, newValue, oldValue) {
                                    changeApplyKind(newValue['P2'], false);
                                }
                            },
                            labelWidth: 100,
                        },
                        
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            store: dutyUserQueryStore,
                            name: 'P4',
                            id: 'P4',
                            labelWidth: mLabelWidth,
                            width: 260,
                            fieldLabel: '值日生',
                            displayField: 'WH_USERNAME',
                            valueField: 'WH_USERID',
                            queryMode: 'local',
                            anyMatch: true,
                            typeAhead: true,
                            forceSelection: true,
                            triggerAction: 'all',
                            multiSelect: false,
                            disabled: true,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_USERNAME}&nbsp;</div></tpl>',
                            listeners: {
                                select: function (oldValue, newValue, eOpts) {
                                    Ext.getCmp('btnDuty').setDisabled(false);
                                }
                            }
                        },
                        {
                            xtype: 'button',
                            text: '設定值日生',
                            id: 'btnDuty',
                            name: 'btnDuty',
                            disabled: true,
                            handler: function () {
                                if (!T1Query.getForm().findField('P4').getValue()) {

                                }

                                Ext.MessageBox.confirm('設定值日生', '是否設定值日生?', function (btn, text) {
                                    if (btn === 'yes') {
                                        var wh_no = T1Query.getForm().findField('P0').getValue();
                                        var userid = T1Query.getForm().findField('P4').getValue();

                                        Ext.Ajax.request({
                                            url: dutyUpdateUrl,
                                            method: reqVal_p,
                                            params: {
                                                WH_NO: wh_no,
                                                WH_USERID: userid
                                            },
                                            success: function (response) {
                                                getDutyUserCombo(wh_no);
                                            },
                                            failure: function (response, options) {
                                                Ext.Msg.alert('失敗', '發生例外錯誤');
                                            }
                                        });

                                    }
                                }
                                );
                            }
                        },
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
                            labelWidth: 180,
                            width: 330
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

                                T1Load('Y');

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
                                f.findField('P2').setValue('2');
                                f.findField('P3').setValue('=');

                                T1Query.getForm().findField('P4').setValue('');
                                T1Query.getForm().findField('P4').disable();
                                Ext.getCmp('btnDuty').setDisabled(true);
                                //f.reset();
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
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '查詢新申請單資料',
                id: 'newApply',
                name: 'newApply',
                hidden:true,
                handler: function () {

                    if (!T1Query.getForm().findField('P0').getValue() ||
                        !T1Query.getForm().findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房</span>與<span style=\'color:red\'>揀貨日期</span>為必填');
                        return;
                    }

                    T31Grid.setHeight(windowHeight / 2 - 30);
                    T32Grid.setHeight(windowHeight / 2 - 30);

                    T31LastRec = null;
                    T31Store.removeAll();
                    T31Load("true");
                    T32Store.removeAll();
                    //myMask.show();
                    newApplyWindow.show();
                }
            },
            {
                text: '複雜度設定',
                id: 'complexity',
                name: 'complexity',
                hidden: true,
                handler: function () {
                    if (!T1Query.getForm().findField('P0').getValue() ||
                        !T1Query.getForm().findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房</span>與<span style=\'color:red\'>揀貨日期</span>為必填');
                        return;
                    }

                    T41Grid.setHeight(windowHeight / 2 - 30);
                    T42Grid.setHeight(windowHeight / 2 - 30);

                    T41LastRec = null;
                    T41Store.removeAll();
                    T41Load();
                    T42Store.removeAll();
                    //myMask.show();
                    complexityWindow.show();
                }
            },
            {
                text: '分配揀貨員',
                id: 'assignUser',
                name: 'assignUser',
                handler: function () {
                    if (apply_kind == '3') {

                        Ext.MessageBox.confirm('分派揀貨單', '是否將揀貨單分配給品項管理人員?', function (btn, text) {
                            if (btn === 'yes') {
                                kind3Distribute();
                            }
                        }
                        );
                       
                        return;
                    }

                    if (!T1Query.getForm().findField('P0').getValue() ||
                        !T1Query.getForm().findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房</span>與<span style=\'color:red\'>揀貨日期</span>為必填');
                        return;
                    }

                    Ext.Ajax.request({
                        url: '/api/CD0002/GetMasterCount',
                        method: '',
                        params: {
                            wh_no: T1Query.getForm().findField('P0').getValue(),
                            pick_date: T1Query.getForm().findField('P1').getValue(),
                            apply_kind: apply_kind
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                var count = data.afrs;
                                if (count == 0) {
                                    Ext.Msg.alert('提醒', '無待分配申請單，請先加入');
                                    return;
                                }

                                if (apply_kind == "2") {
                                    if (dutyUser == '') {
                                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>值日生</span>尚未指定');
                                        return;
                                    }
                                    // 查詢是否已排有臨時申請單揀貨批次資料(如max_logno>1000則表示已有安排，否則沒有)
                                    tempDistribution();
                                } else {
                                    T51Store.removeAll();

                                    T51Query.getForm().findField('T51P0').setValue(T1Query.getForm().findField('P0').getValue());
                                    T51Query.getForm().findField('T51P1').setValue(T1Query.getForm().findField('P1').rawValue);
                                    T51Query.getForm().findField('T51P2').setValue('');
                                    //myMask.show();
                                    distributionWindow.show();
                                }
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
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

                    if (!T1Query.getForm().findField('P0').getValue() &&
                        !T1Query.getForm().findField('P1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房</span>與<span style=\'color:red\'>揀貨日期</span>為必填');
                        return;
                    }

                    clearAssign();
                }
            }
        ]
    });
    var kind3Distribute = function () {
        // 
        Ext.Ajax.request({
            url: '/api/CD0002/SetKind3Sheets',
            method: reqVal_p,
            params: {
                WH_NO: T1Query.getForm().findField('P0').getValue(),
                PICK_DATE: T1Query.getForm().findField('P1').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel("揀貨單分配成功");
                    T1Load('Y');
                } else {
                    Ext.Msg.alert('提示', data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }
    var T1RowEditing = Ext.create('Ext.grid.plugin.RowEditing', {
        //clicksToMoveEditor: 1,
        clicksToEdit: 1,
        autoCancel: false,
        saveBtnText: '更新',
        cancelBtnText: '取消',
        errorsText: '錯誤訊息',
        dirtyText: '請按更新以修改資料或取消變更',
        listeners: {
            edit: function (editor, context, eOpts) {
                
                //updateComplexity(T41LastRec);
                updateComplexity(context.record.data);

            },
            canceledit: function (editor, context, eOpts) {
            },
            beforeedit: function (editor, context, eOpts) {
                
                if (context.record.data.LOT_NO.trim() != '') {
                    return false;
                }

                if (apply_kind == '2' || apply_kind == '3') {
                    //editor.getEditor().floatingButtons.hide();
                    return false;
                }
            }
        }
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        plugins: [T1RowEditing],
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
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "入庫庫房",
                dataIndex: 'INID',
                width: 80
            },
            {
                text: "入庫庫房名稱",
                dataIndex: 'INID_NAME',
                width: 100
            },
            {
                text: "申請單數量",
                dataIndex: 'DOCNO_COUNTS',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "申請品項數",
                dataIndex: 'MMCODE_COUNTS',
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
                text: "複雜度",
                dataIndex: 'COMPLEXITY',
                width: 80,
                align: 'right',
                style: 'text-align:left',
                editor: {
                    xtype: 'numberfield',
                    maxLength: 15,
                    minValue: 0.1,
                    hideTrigger: true,
                    allowDecimals: true,
                    forcePrecision: true,
                    decimalPrecision: 1
                }
            },
            {
                text: "揀貨批次",
                dataIndex: 'LOT_NO',
                width: 80
            },
            {
                text: "揀貨人員",
                dataIndex: 'PICKER_NAME',
                width: 120,
                //renderer: function (val, meta, record) {
                //    return record.data.PICK_USERID + ' ' +record.data.PICKER_NAME;
                //},
            },
            //{
            //    text: "申請單備註",
            //    dataIndex: 'APPLY_NOTE',
            //    width: 150
            //},
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                T1LastRec = record;
                T2Load();
            },
            beforeedit: function (editor, context, eOpts) {
                if (apply_kind == '2' || apply_kind == '3') {
                    editor.getEditor().floatingButtons.hide();
                }
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
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 155,
            },
            {
                text: "項次",
                dataIndex: 'SEQ',
                width: 50,
                align: 'right',
                style: 'text-align:left',
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
                align: 'right',
                style: 'text-align:left'
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
    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }
    function changeApplyKind(applyKind, init) {
        apply_kind = applyKind;
        
        
        if (applyKind == '2') {     //臨時
            if (T1Query.getForm().findField('P0').getValue() &&
                T1Query.getForm().findField('P4').getValue()) {
                Ext.getCmp('btnDuty').enable();
            }
            T1Query.getForm().findField('P4').enable();
            //Ext.getCmp('complexity').disable();
            //Ext.getCmp('assignUser').disable();
        } else if (apply_kind == '1') {
            Ext.getCmp('btnDuty').disable();
            Ext.getCmp('complexity').enable();
            T1Query.getForm().findField('P4').disable();
        } else {
            Ext.getCmp('btnDuty').disable();
            T1Query.getForm().findField('P4').disable();
        }

        if (init) {
            var columns = T1Grid.getColumns();
            var index = getColumnIndex(columns, 'COMPLEXITY');

            if (apply_kind == '2' || apply_kind == '3') {     //臨時
                if (index >= 0) {
                    T1Grid.getColumns()[index].hide();
                }
                //Ext.getCmp('assignUser').disable();
            } else {
                if (index >= 0) {
                    T1Grid.getColumns()[index].show();
                }
            }
        }
    }
    

    function changeAssignType(type) {
        if (type == "=") {  //未分配
            Ext.getCmp('assignUser').enable();
            Ext.getCmp('assignedDocs').disable();
            Ext.getCmp('clearAssign').disable();
        } else {
            Ext.getCmp('assignUser').disable();
            Ext.getCmp('assignedDocs').enable();
            Ext.getCmp('clearAssign').enable();

        }
    }
    //#endregion --------------------

    //#region ------ 查詢新申請單資料 ------
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
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        //collapsible: true,
        //title: '明細',
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

        Ext.Ajax.request({
            url: TransferTobcwhpickUrl,
            method: reqVal_p,
            params: {
                WH_NO: wh_no,
                PICK_DATE: pick_date,
                DOCNO: docno
            },
            success: function (response) {
                //newApplyWindow.hide();
                //T1Load();
                T31Load("false");
                T32Store.removeAll();
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
                    minHeight: windowHeight / 2 - 60,
                    items: [T31Grid]
                },
                    {
                        xtype: 'splitter',
                        collapseTarget: 'dev'
                    },{
                    xtype: 'panel',
                    autoScroll: true,
                    itemId: 't32Grid',
                    region: 'south',
                    layout: 'fit',
                    //collapsible: true,
                    //title: '明細',
                    //titleCollapse: true,
                    border: false,
                    height: '50%',
                    split: true,
                    items: [T32Grid]
                    }
                ],
            }
        ],
        //items: [
        //    {
        //        xtype: 'container',
        //        layout: 'fit',
        //        items: [{
        //            itemId: 't31Grid',
        //            region: 'center',
        //            layout: 'fit',
        //            collapsible: false,
        //            title: '',
        //            border: false,
        //            height: '50%',
        //            split: true,
        //            items: [T31Grid]
        //        }
        //            , {
        //            itemId: 't32Grid',
        //            region: 'south',
        //            layout: 'fit',
        //            collapsible: true,
        //            title: '明細',
        //            border: false,
        //            height: '50%',
        //            split: true,
        //            items: [T32Grid]
        //            }],
        //    }
        //    ],
        ////items:[T31Grid, T32Grid],
        width: "700px",
        height: windowHeight,
        resizable: false,
        closable: false,
        y: 0,
        title: "新申請單資料",
        buttons: [{
            text: '關閉',
            handler: function () {
                this.up('window').hide();
                //myMask.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                newApplyWindow.setY(0);
            }
        }
    });
    newApplyWindow.hide();
    //#endregion --------------------


    //#region ------ 複雜度設定 ------ 留有updateComplexity功能
    //var T41Store = viewModel.getStore('ComplexityMaster');
    //function T41Load(runDelete) {
    //    if (T1Query.getForm().findField('P0').getValue() &&
    //        T1Query.getForm().findField('P1').getValue()) {

    //        T42Store.removeAll();

    //        T41Query.getForm().findField('T41P0').setValue(T1Query.getForm().findField('P0').getValue());
    //        T41Query.getForm().findField('T41P1').setValue(T1Query.getForm().findField('P1').rawValue);

    //        T41Store.getProxy().setExtraParam("WH_NO", T1Query.getForm().findField('P0').getValue().split(' ')[0]);
    //        T41Store.getProxy().setExtraParam("PICK_DATE", T1Query.getForm().findField('P1').getValue());
    //        T41Tool.moveFirst();
    //    }
    //}
    //var T42Store = viewModel.getStore('ComplexityDetail');
    //function T42Load() {
    //    if (T41LastRec != null) {
    //        T42Store.getProxy().setExtraParam("WH_NO", T1Query.getForm().findField('P0').getValue().split(' ')[0]);
    //        T42Store.getProxy().setExtraParam("PICK_DATE", T1Query.getForm().findField('P1').getValue());
    //        T42Store.getProxy().setExtraParam("DOCNO", T41LastRec.data["DOCNO"]);
    //        T42Tool.moveFirst();
    //    }
    //}
    //var T41Query = Ext.widget({
    //    xtype: 'form',
    //    layout: 'form',
    //    border: false,
    //    autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
    //    items: [{
    //        xtype: 'container',
    //        layout: 'hbox',
    //        items: [
    //            {
    //                xtype: 'panel',
    //                id: 'PanelP41',
    //                border: false,
    //                layout: 'hbox',
    //                items: [
    //                    {
    //                        xtype: 'displayfield',
    //                        name: 'T41P0',
    //                        id: 'T41P0',
    //                        labelWidth: 60,
    //                        width: 200,
    //                        fieldLabel: '庫房',
    //                        labelAlign: 'right',
    //                    },
    //                    {
    //                        xtype: 'displayfield',
    //                        fieldLabel: '揀貨日期',
    //                        name: 'T41P1',
    //                        id: 'T41P1',
    //                        labelWidth: 60,
    //                        width: 150,
    //                        padding: '0 4 0 4',
    //                        labelAlign: 'right',
    //                    }
    //                ]
    //            }
    //        ]
    //    }]
    //});
    //var T41Tool = Ext.create('Ext.PagingToolbar', {
    //    store: T41Store,
    //    border: false,
    //    plain: true,
    //    displayInfo: true
    //});
    //var T41RowEditing = Ext.create('Ext.grid.plugin.RowEditing', {
    //    //clicksToMoveEditor: 1,
    //    clicksToEdit: 1,
    //    autoCancel: false,
    //    saveBtnText: '更新',
    //    cancelBtnText: '取消',
    //    errorsText: '錯誤訊息',
    //    dirtyText: '請按更新以修改資料或取消變更',
    //    listeners: {
    //        edit: function (editor, context, eOpts) {
                
    //            //updateComplexity(T41LastRec);
    //            updateComplexity(context.record.data);
                
    //        },
    //        canceledit: function (editor, context, eOpts) {
    //        }
    //    }
    //});
    //var T41Grid = Ext.create('Ext.grid.Panel', {
    //    store: T41Store,
    //    //plain: true,
    //    loadingText: '處理中...',
    //    loadMask: true,
    //    cls: 'T1',
    //    height: windowHeight / 2 - 30,
    //    plugins: [T41RowEditing],
    //    dockedItems: [
    //        //{
    //        //    dock: 'top',
    //        //    xtype: 'toolbar',
    //        //    items: [T41Query]
    //        //},
    //        {
    //            dock: 'top',
    //            xtype: 'toolbar',
    //            items: [T41Tool]
    //        }
    //    ],
    //    columns: [
    //        {
    //            xtype: 'rownumberer'
    //        },
    //        {
    //            text: "申請單號碼",
    //            dataIndex: 'DOCNO',
    //            width: 155
    //        },
    //        {
    //            text: "申請部門代碼",
    //            dataIndex: 'APPDEPT_NAME',
    //            width: 120
    //        },
    //        {
    //            text: "申請品項數",
    //            dataIndex: 'APPCNT',
    //            width: 90,
    //            align: 'right',
    //            style: 'text-align:left',
    //        },
    //        {
    //            text: "申請總數量",
    //            dataIndex: 'APPQTY_SUM',
    //            width: 90,
    //            align: 'right',
    //            style: 'text-align:left',
    //        },
    //        {
    //            text: "複雜度",
    //            dataIndex: 'COMPLEXITY',
    //            width: 90,
    //            align: 'right',
    //            style: 'text-align:left',
    //            editor: {
    //                xtype: 'numberfield',
    //                maxLength: 15,
    //                minValue:0.1,
    //                hideTrigger: true,
    //                allowDecimals: true,
    //                //forcePrecision: true,
    //                decimalPrecision: 0

    //            }
    //        },
    //        //{
    //        //    flex: 1,
    //        //    renderer: function (val, meta, record) {

    //        //        var mmcode = record.data.MMCODE;
    //        //        var param3 = record.data.DOCNO;
    //        //        return '<a href=javascript:void(0)>更新資料</a>';
    //        //    },
    //        //    width: 120
    //        //}
    //    ],
    //    //plugins: [
    //    //    Ext.create('Ext.grid.plugin.CellEditing', {
    //    //        clicksToEdit: 1
    //    //    })
    //    //],
    //    listeners: {
    //        itemclick: function (self, record, item, index, e, eOpts) {

    //            T41LastRec = record;
    //            if (T41cell == '') {
    //                T42Load();
    //            }
    //            T41cell = '';
    //        },
    //        //cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {

    //        //    if (cellIndex != 6) {
    //        //        return;
    //        //    }
    //        //    T41LastRec = record;
    //        //    T41cell = 'cell';

    //        //    updateComplexity(T41LastRec);
    //        //},
    //    }
    //});
    //var T42Tool = Ext.create('Ext.PagingToolbar', {
    //    store: T42Store,
    //    border: false,
    //    plain: true,
    //    displayInfo: true
    //});
    //var T42Grid = Ext.create('Ext.grid.Panel', {
    //    store: T42Store,
    //    //plain: true,
    //    loadingText: '處理中...',
    //    loadMask: true,
    //    cls: 'T1',
    //    height: windowHeight / 2 - 35,
    //    dockedItems: [
    //        {
    //            dock: 'top',
    //            xtype: 'toolbar',
    //            items: [T42Tool]
    //        }
    //    ],
    //    columns: [
    //        //{
    //        //    xtype: 'rownumberer'
    //        //},
    //        {
    //            text: "項次",
    //            dataIndex: 'SEQ',
    //            width: 50
    //        },
    //        {
    //            text: "院內碼",
    //            dataIndex: 'MMCODE',
    //            width: 110
    //        },
    //        {
    //            text: "中文品名",
    //            dataIndex: 'MMNAME_C',
    //            width: 150
    //        },
    //        {
    //            text: "英文品名",
    //            dataIndex: 'MMNAME_E',
    //            width: 150
    //        },
    //        {
    //            text: "核撥量",
    //            dataIndex: 'APPQTY',
    //            width: 100,
    //            align: 'right',
    //            style: 'text-align:left',
    //        },
    //        {
    //            text: "最小撥補單位",
    //            dataIndex: 'BASE_UNIT',
    //            width: 90
    //        }
    //    ]
    //});
    function updateComplexity(record) {
        Ext.Ajax.request({
            url: complexityUpdateUrl,
            method: reqVal_p,
            params: {
                wh_no: T1Query.getForm().findField('P0').getValue(),
                pick_date: T1Query.getForm().findField('P1').getValue(),
                complexity: record.COMPLEXITY,
                inid: T1LastRec.data["INID"],
                docno_counts: T1LastRec.data["DOCNO_COUNTS"],
                docnos: T1LastRec.data["DOCNOS"],
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel("複雜度更新成功");
                    T1Load('N');
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }
    //var complexityWindow = Ext.create('Ext.window.Window', {
    //    renderTo: Ext.getBody(),
    //    modal: true,
    //    //items: [T41Grid, T42Grid],
    //    items: [
    //        {
    //            xtype: 'container',
    //            layout: 'fit',
    //            items: [
    //                T41Query,
    //                {
    //                xtype: 'panel',
    //                itemId: 't41Grid',
    //                region: 'center',
    //                layout: 'fit',
    //                collapsible: false,
    //                title: '',
    //                border: false,
    //                height: '50%',
    //                minHeight: windowHeight / 2 - 60,
    //                items: [T41Grid]
    //            },
    //            {
    //                xtype: 'splitter',
    //                collapseTarget: 'dev'
    //            }, {
    //                xtype: 'panel',
    //                autoScroll: true,
    //                itemId: 't42Grid',
    //                region: 'south',
    //                layout: 'fit',
    //                //collapsible: true,
    //                //title: '明細',
    //                //titleCollapse: true,
    //                border: false,
    //                height: '50%',
    //                split: true,
    //                items: [T42Grid]
    //            }
    //            ],
    //        }
    //    ],
    //    width: "700px",
    //    height: windowHeight,
    //    resizable: false,
    //    closable: false,
    //    y: 0,
    //    title: "複雜度設定",
    //    buttons: [{
    //        text: '關閉',
    //        handler: function () {
    //            this.up('window').hide();
    //            //myMask.hide();
    //        }
    //    }],
    //    listeners: {
    //        show: function (self, eOpts) {
    //            complexityWindow.setY(0);
    //        }
    //    }
    //});
    //complexityWindow.hide();
    //#endregion --------------------

    //#region ------ 分配揀貨員 ------
    var T51Store = viewModel.getStore('DistributeRegular');
    function T51Load(runDelete) {
        T51Store.removeAll();

        if (T1Query.getForm().findField('P0').getValue() &&
            T1Query.getForm().findField('P1').getValue()) {


            T51Query.getForm().findField('T51P0').setValue(T1Query.getForm().findField('P0').getValue());
            T51Query.getForm().findField('T51P1').setValue(T1Query.getForm().findField('P1').rawValue);

            T51Store.getProxy().setExtraParam("WH_NO", T1Query.getForm().findField('P0').getValue().split(' ')[0]);
            T51Store.getProxy().setExtraParam("PICK_DATE", T1Query.getForm().findField('P1').getValue());
            T51Store.getProxy().setExtraParam("USER_COUNT", T51Query.getForm().findField('T51P2').getValue());
            T51Tool.moveFirst();
        }
    }
    var T51Query = Ext.widget({
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
                    id: 'PanelP51',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            name: 'T51P0',
                            id: 'T51P0',
                            labelWidth: 60,
                            width: 200,
                            fieldLabel: '庫房',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '揀貨日期',
                            name: 'T51P1',
                            id: 'T51P1',
                            labelWidth: 60,
                            width: 150,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '分配人數',
                            name: 'T51P2',
                            id: 'T51P2',
                            labelWidth: 60,
                            width: 150,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                            fieldCls: 'required',
                            enforceMaxLength: false,
                            maxLength: 14,
                            hideTrigger: true
                        },
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
                        var item = {
                            WH_NO: tempData[i].data.WH_NO,
                            PICK_DATE: tempData[i].data.PICK_DATE,
                            LOT_NO: tempData[i].data.LOT_NO,
                            PICK_USERID: tempData[i].data.PICK_USERID,
                            CALC_TIME: tempData[i].data.CALC_TIME,
                        }
                        data.push(item);
                    }
                    Ext.Ajax.request({
                        url: '/api/CD0002/SetPickUser',
                        method: reqVal_p,
                        params: { ITEM_STRING: Ext.util.JSON.encode(data) },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel("儲存成功");
                                distributionWindow.hide();

                                T1Query.getForm().findField('P2').setValue('1');
                                T1Query.getForm().findField('P3').setValue('<>');
                                T1Load('Y');

                                distributionWindow.hide();
                                //myMask.hide();
                                msglabel("揀貨員分配成功");
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                }
            }]
    });
    var T51Grid = Ext.create('Ext.grid.Panel', {
        store: T51Store,
        //plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 350,
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
                text: "揀貨批次",
                dataIndex: 'LOT_NO',
                width: 90
            },
            {
                text: "申請單數",
                dataIndex: 'DOCNO_SUM',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "申請品項數",
                dataIndex: 'APPITEM_SUM',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "複雜度合計",
                dataIndex: 'COMPLEXITY_SUM',
                width: 90,
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
            }
        }
    });
    var T51Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            //labelAlign: 'right'
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP52',
            border: false,
            layout: 'vbox',
            items: [
                {
                    xtype: 'displayfield',
                    width: 200,
                    value: '已有排定臨時申請單揀貨批次，要新增揀貨批次？還是納入最後一批揀貨批次中？',
                },
                {
                    xtype: 'container',
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'panel',
                            id: 'PanelP521',
                            border: false,
                            layout: 'hbox',
                            items: [
                                {
                                    xtype: 'button',
                                    text: '新增揀貨批次',
                                    handler: function () {
                                        setTempSheets(false);
                                    }
                                },
                                {
                                    xtype: 'button',
                                    text: '納入最後一批揀貨批次',
                                    handler: function () {
                                        setTempSheets(true);
                                    }
                                }
                            ]
                        }
                    ]
                }
            ]
        }]
    });
    var distributionWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        items: [T51Query,T51Grid],
        width: "650px",
        height: "410px",
        resizable: false,
        closable: false,
        title: "分配揀貨員",
        modal: true,
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
                distributionWindow.setY(10);
            }
        }
    });
    distributionWindow.hide();
    var hasTempAddNewWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T51Form],
        resizable: false,
        closable: false,
        width: "300px",
        height: "200px",
        title: "已有排定臨時申請單揀貨批次"
    });
    hasTempAddNewWindow.hide();
    // 設定臨時申請單揀貨批次資料
    function setTempSheets(insertLast) {
        Ext.Ajax.request({
            url: '/api/CD0002/SetTempSheets',
            method: reqVal_p,
            params: {
                WH_NO: T1Query.getForm().findField('P0').getValue(),
                PICK_DATE: T1Query.getForm().findField('P1').getValue(),
                DUTY_USER: T1Query.getForm().findField('P4').getValue(),
                MAX_LOT_NO: max_temp_lot_no,
                INSERT_LAST: insertLast ? "true" : "false"
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel("臨時單分配成功");
                    //myMask.hide();
                    hasTempAddNewWindow.hide();
                    T1Load('N');
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function tempDistribution() {
        Ext.Ajax.request({
            url: '/api/CD0002/GetTempPicklotNo',
            method: reqVal_p,
            params: {
                WH_NO: T1Query.getForm().findField('P0').getValue(),
                PICK_DATE: T1Query.getForm().findField('P1').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {

                    max_temp_lot_no = data.afrs;

                    if (max_temp_lot_no <= 1000) {
                        var msg = "將排定值日生 " + dutyUserName + " 揀貨批次1001號負責這批臨時申請單?";
                        Ext.MessageBox.confirm('', msg, function (btn, text) {
                            if (btn === 'yes') {
                                // insertFirstTemp("");
                                setTempSheets(false);
                            }
                        });
                    } else {
                        hasTempAddNewWindow.show();
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    //#endregion --------------------

    //#region ------ 已排揀貨批次 ------
    var T61Store = viewModel.getStore('DistributedMaster');
    T61Store.on('load', function (store, options) {
        var columns = T61Grid.getColumns();
        var index = getColumnIndex(columns, 'PICK_USERNAME');

        if (apply_kind == '3') {     //臨時
            if (index >= 0) {
                T61Grid.getColumns()[index].hide();
            }
            //Ext.getCmp('assignUser').disable();
        } else {
            if (index >= 0) {
                T61Grid.getColumns()[index].show();
            }
        }
    });
    function T61Load(runDelete) {
        T61Store.removeAll();

        if (T1Query.getForm().findField('P0').getValue() &&
            T1Query.getForm().findField('P1').getValue()) {
            
            T61Query.getForm().findField('T61P0').setValue(T1Query.getForm().findField('P0').getValue());
            T61Query.getForm().findField('T61P1').setValue(T1Query.getForm().findField('P1').rawValue);
            //var p2 = T1Query.getForm().findField('P2').getValue()['P2'];
            if (apply_kind == "3") {
                //T61Query.getForm().findField('T61P2').setValue('T61P2Temp', true);
                T61Query.getForm().findField('T61P2').setValue("被服與資訊耗材");
            }else if (apply_kind == "2") {
                //T61Query.getForm().findField('T61P2').setValue('T61P2Temp', true);
                T61Query.getForm().findField('T61P2').setValue("臨時");
            } else {
                //T61Query.getForm().findField('T61P2').setValue('T61P2Reg', true);
                T61Query.getForm().findField('T61P2').setValue("常態");
            }
            //T61Query.getForm().findField('T61P2').setValue(p2);

            T61Store.getProxy().setExtraParam("WH_NO", T1Query.getForm().findField('P0').getValue().split(' ')[0]);
            T61Store.getProxy().setExtraParam("PICK_DATE", T1Query.getForm().findField('P1').getValue());
            T61Store.getProxy().setExtraParam("APPLY_KIND", apply_kind);
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
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '申請類別',
                            name: 'T61P2',
                            id: 'T61P2',
                            labelWidth: 60,
                            width: 200,
                            padding: '0 4 0 4'
                        },
                        {
                            xtype: 'radiogroup',
                            name: 'T61P3',
                            fieldLabel: '排序方式',
                            items: [
                                { boxLabel: '申請單號項次', name: 'T61P3', inputValue: 'docno,seq', width: 90 },
                                { boxLabel: '英文品名', name: 'T61P3', inputValue: 'mmname_e', width: 70 },
                                { boxLabel: '儲位', name: 'T61P3', inputValue: 'store_loc', width: 50 , checked: true }
                            ],
                            listeners: {
                                change: function (field, newValue, oldValue) {
                                    //changeApplyKind(newValue['P2']);
                                    if (T61LastRec != null) {
                                        T62Load();
                                    }
                                }
                            },
                            labelWidth: 60,
                            //width: 250
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
        buttons: [
            {
                text: '列印揀貨單',
                id: 'btnShowReport',
                name: 'btnShowReport',
                handler: function () {
                    var selection = T61Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要列印的揀貨批次');
                        return;
                    }
                    var lotnos = '';
                    for (var i = 0; i < selection.length; i++) {
                        if (lotnos != '') {
                            lotnos += '|';
                        }
                        lotnos += selection[i].data.LOT_NO
                    }

                    showReport(lotnos);
                }
            },
            {
                text: '取消此批揀貨',
                id: 'btnCancelDistributed',
                name: 'btnCancelDistributed',
                handler: function () {
                    var selection = T61Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要取消的揀貨批次');
                        return;
                    }
                    var items = [];
                    for (var i = 0; i < selection.length; i++) {
                        var data = {
                            WH_NO: selection[i].data.WH_NO,
                            PICK_DATE: selection[i].data.PICK_DATE,
                            LOT_NO: selection[i].data.LOT_NO,
                        };
                        items.push(data);
                    }
                    cancelDistributed(items);
                }
            },
        ]
    });
    var T61Grid = Ext.create('Ext.grid.Panel', {
        store: T61Store,
        //plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight / 2 - 60,
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
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
                text: "揀貨人員",
                dataIndex: 'PICK_USERNAME',
                width: 180
            },
            {
                text: "申請單數",
                dataIndex: 'APP_CNT',
                width: 90
            },
            //{
            //    text: "",
            //    renderer: function (val, meta, record) {
            //        return '<a href=javascript:void(0)>列印揀貨單</a>';
            //    },
            //    width: 80
            //},
            //{
            //    text: "",
            //    renderer: function (val, meta, record) {
            //        return '<a href=javascript:void(0)>取消此批揀貨</a>';
            //    },
            //    width: 90
            //},
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
            //cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                
            //    if (cellIndex != 2 && cellIndex != 3) {
            //        return;
            //    }
            //    // T61LastRec = record;
            //    T61cell = 'cell';

            //    if (cellIndex == 3) {
            //        // CancelDistributed
            //        Ext.Ajax.request({
            //            url: '/api/CD0002/CancelDistributed',
            //            method: reqVal_p,
            //            params: {
            //                WH_NO: record.data.WH_NO,
            //                PICK_DATE: record.data.PICK_DATE,
            //                LOT_NO: record.data.LOT_NO
            //            },
            //            success: function (response) {
            //                var data = Ext.decode(response.responseText);
            //                if (data.success) {
            //                    msglabel("取消此批揀貨成功");
            //                    T61Load();
            //                }
            //            },
            //            failure: function (response, options) {

            //            }
            //        });
            //    } else {
            //        showReport(record);
            //    }

            //    //T62Load();
            //},
        }
    });
    var T62Store = viewModel.getStore('DistributedDetail');
    T62Store.on('load', function (store, options) {
        var columns = T62Grid.getColumns();
        var index = getColumnIndex(columns, 'PICK_USERNAME');

        if (apply_kind == '3') {     //臨時
            if (index >= 0) {
                T62Grid.getColumns()[index].show();
            }
            //Ext.getCmp('assignUser').disable();
        } else {
            if (index >= 0) {
                T62Grid.getColumns()[index].hide();
            }
        }
    });
    function T62Load() {
        if (T61LastRec != null) {
            T62Store.getProxy().setExtraParam("WH_NO", T1Query.getForm().findField('P0').getValue().split(' ')[0]);
            T62Store.getProxy().setExtraParam("PICK_DATE", T1Query.getForm().findField('P1').getValue());
            T62Store.getProxy().setExtraParam("LOT_NO", T61LastRec.data["LOT_NO"]);
            T62Store.getProxy().setExtraParam("SORTER", T61Query.getForm().findField('T61P3').getValue()['T61P3']);
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
            {
                text: "入庫庫房",
                dataIndex: 'APPDEPT',
                width: 80
            },
            {
                text: "入庫庫房名稱",
                dataIndex: 'APPDEPTNAME',
                width: 155
            },
            {
                text: "申請單號碼",
                dataIndex: 'DOCNO',
                width: 155
            },
            {
                text: "項次",
                dataIndex: 'SEQ',
                width: 50,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "物料分類",
                dataIndex: 'MAT_CLASS',
                width: 80
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 110
            },
            {
                text: "揀貨人員",
                dataIndex: 'PICK_USERNAME',
                width: 90
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
                width: 70,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "撥補單位",
                dataIndex: 'BASE_UNIT',
                width: 80
            },
            {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                width: 90
            },
            {
                text: "備註",
                dataIndex: 'APLYITEM_NOTE',
                width: 150
            },
            
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
        width: "910px",
        height: windowHeight,
        resizable: false,
        closable: false,
        y: 0,
        title: "已排揀貨批次",
        buttons: [{
            text: '關閉',
            handler: function () {
                this.up('window').hide();
                //myMask.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                distritedListnWindow.setY(0);
            }
        }
    });
    distritedListnWindow.hide();

    function showReport(lotnos) {
        
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?WH_NO=' + T1Query.getForm().findField('P0').getValue()
                + '&PICK_DATE=' + dateTransform(T1Query.getForm().findField('P1').getValue())
                + '&LOT_NOS=' + lotnos
                + '&SORTER=' + T61Query.getForm().findField('T61P3').getValue()['T61P3']
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
    function cancelDistributed(items) {
                Ext.Ajax.request({
                        url: '/api/CD0002/CancelDistributed',
                        method: reqVal_p,
                        params: {
                            ITEM_STRING: Ext.util.JSON.encode(items)
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
    }

    function dateTransform(date) {
        var yyyy = date.getFullYear().toString();
        var m = (date.getMonth() + 1).toString();
        var d = date.getDate().toString();
        var mm = m.length < 2 ? "0" + m : m;
        var dd = d.length < 2 ? "0" + d : d;
        return yyyy + "-" + mm + "-" + dd;
    }
    //#endregion --------------------

    //#region ------ 全部重新分配 ------
    function clearAssign() {
        var msg = "是否確定要刪除所有";
        if (apply_kind == "3") {
            msg += "被服與資訊耗材"
        } else if (apply_kind == "2") {
            msg += "臨時"
        } else {
            msg += "常態"
        }
        msg += "已分配揀貨資料?";
        Ext.MessageBox.confirm('全部重新分配', msg, function (btn, text) {
            if (btn === 'yes') {
                
                var wh_no = T1Query.getForm().findField('P0').getValue();
                var pick_date = T1Query.getForm().findField('P1').getValue();
                var apply_kind = T1Query.getForm().findField('P2').getValue()['P2'];

                Ext.Ajax.request({
                    url: '/api/CD0002/ClearDistribution',
                    method: reqVal_p,
                    params: {
                        WH_NO: wh_no,
                        PICK_DATE: pick_date,
                        APPLY_KIND: apply_kind
                    },
                    success: function (response) {
                        msglabel("全部重新分配成功")
                        T1Load('N');
                    },
                    failure: function (response, options) {
                        Ext.Msg.alert('失敗', '發生例外錯誤');
                    }
                });

            }
        }
        );
    }

    //#endregion --------------------

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
                ]
            }
        ]
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '' });
    myMask.hide();

    changeApplyKind('2', true);
    changeAssignType('=');
    T1Query.getForm().findField('P4').disable();

    Ext.on('resize', function () {
        windowWidth = $(window).width();
        windowHeight = $(window).height();
        newApplyWindow.setHeight(windowHeight);
        T31Grid.setHeight(windowHeight / 2 - 30);
        T32Grid.setHeight(windowHeight / 2 - 30);

        //complexityWindow.setHeight(windowHeight);
        //T41Grid.setHeight(windowHeight / 2 - 30);
        //T42Grid.setHeight(windowHeight / 2 - 30);

        distritedListnWindow.setHeight(windowHeight);
        T61Grid.setHeight(windowHeight / 2 - 60);
        T62Grid.setHeight(windowHeight / 2 - 45);

    });
});