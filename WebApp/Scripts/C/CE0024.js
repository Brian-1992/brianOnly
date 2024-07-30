Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var windowWidth = $(window).width();
    var windowHeight = $(window).height();

    var T1Name = '藥局抽盤品項設定檔資料維護';

    ////// 篩選條件
    ////// 誤差量大於
    ////// 誤差金額大於
    ////// 進價大於
    ////// 誤差百分比大於
    ////// 管制藥、高價藥、罕見藥、CDC


    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    //var userId, userName, userInid, userInidName;

    var viewModel = Ext.create('WEBAPP.store.CE0024VM');

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;

    var isFinish = false;
    var todayDateString = '';
    var sCHK_STATUS = null;

    // ------ 主畫面 ------

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var whnoQueryStore1 = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: '/api/CE0015/GetWhnos',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        whnoQueryStore1.add({ VALUE: '', TEXT: '全部' });
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add(wh_nos[i]);
                            whnoQueryStore1.add(wh_nos[i]);
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
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
    function getAploutqtyDateRange() {
        Ext.Ajax.request({
            url: '/api/CE0024/AploutqtyDateRange',
            method: reqVal_p,
            params: { chk_ym: T1Query.getForm().findField('P1').rawValue },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('P4').setValue('<span style="color:blue">' + data.msg+'</span>');
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function isEditable(chk_ym) {

        if (chk_ym.substring(0, 5) != todayDateString) {
            return false;
        } else {
            return true;
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
                            fieldLabel: '庫房代碼',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            fieldCls: 'required',
                            anyMatch: true,
                            allowBlank: false,
                            typeAhead: true,
                            forceSelection: true,
                            triggerAction: 'all',
                            multiSelect: false,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                        },
                        {
                            xtype: 'monthfield',
                            fieldLabel: '盤點年月',
                            name: 'P1',
                            id: 'P1',
                            // enforceMaxLength: true,
                            //maxLength: 5,
                            //minLength: 5,
                            //regexText: '請填入民國年月',
                            //regex: /\d{5,5}/,
                            labelWidth: mLabelWidth,
                            fieldCls: 'required',
                            width: 180,
                            padding: '0 4 0 4',
                            //format: 'Xm',
                            minDate: new Date(),
                            value: new Date(),
                            //listeners: {
                            //    change: function (self, newValue, oldValue) {
                            //        
                            //        //var temp = newValue;
                            //        //temp = new Date(temp.setMonth(temp.getMonth() - 1));
                            //        //T1Query.getForm().findField('P00').setValue(temp);
                            //        resetFilter();

                            //        T21Store.getProxy().setExtraParam({});
                            //        T22Store.getProxy().setExtraParam({});
                            //        T23Store.getProxy().setExtraParam({});

                            //        setP00Value(newValue);
                            //        T1Load();
                            //        T21Load();
                            //        T22Load();
                            //        T23Load();
                            //        chknoExists();
                            //    }
                            //}
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');
                                var f = T1Query.getForm();
                                if (!f.findField('P0').getValue() ||
                                    !f.findField('P1').getValue()) {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>與<span style=\'color:red\'>盤點年月</span>需填寫');
                                    return;
                                }
                                getAploutqtyDateRange();
                                T1Load();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '盤盈虧現況查詢',
                            handler: function () {
                                
                                if (!T1Query.getForm().findField('P1').getValue()) {
                                    Ext.Msg.alert('提醒', '請填寫<span style=\'color:red\'>盤點年月</span>');
                                    return;
                                }
                                
                                if (!T1Query.getForm().findField('P0').getValue()) {
                                    T11Query.getForm().findField('T11P2').setValue('');
                                } else {
                                    T11Query.getForm().findField('T11P2').setValue(T1Query.getForm().findField('P0').getValue());
                                }

                                T11Load();
                                T12Load();
                                T11Query.getForm().findField('T11P0').setValue(T1Query.getForm().findField('P1').rawValue);

                                chkResultWindow.setHeight(windowHeight);
                                chkResultWindow.setWidth(windowWidth);
                                
                                T11Grid.setHeight(windowHeight - 180 - 40);
                                chkResultWindow.show();
                            }
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '目前盤點狀態',
                            name: 'P2',
                            id:'P2',
                            value:''
                        },
                        {
                            xtype: 'displayfield',
                            name: 'P3',
                            id: 'P3',
                            padding: '0 0 0 5',
                            value: '<span style="color:red">本月盤點已完成，無法修改資料</span>',
                            hidden:true
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '',
                            name: 'P4',
                            id: 'P4',
                            padding: '0 0 0 5',
                            value: ''
                        },
                    ]
                }
            ]
        }]
    });
    var T1Store = viewModel.getStore('All');
    function T1Load() {
        
        T1Store.getProxy().setExtraParam("wh_no", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("chk_ym", T1Query.getForm().findField('P1').rawValue);
        //T1Tool.moveFirst();
        T1Store.load();
    }
    T1Store.on('load', function (store, options) {
        
        Ext.getCmp('recheck').enable();
        Ext.getCmp('mark').enable();
        Ext.getCmp('finish').enable();

        Ext.Ajax.request({
            url: '/api/CE0024/GetIfFinish',
            method: reqVal_p,
            params: {
                chk_ym: T1Query.getForm().findField('P1').rawValue
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    isFinish = (data.msg == "Y");
                    if (isFinish) {
                        T1Query.getForm().findField('P3').show();
                    } else {
                        T1Query.getForm().findField('P3').hide();
                    }
                }

                Ext.Ajax.request({
                    url: '/api/CE0024/ChknoExists',
                    method: reqVal_p,
                    params: {
                        chk_ym: T1Query.getForm().findField('P1').rawValue,
                        wh_no: T1Query.getForm().findField('P0').getValue()
                    },
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            if (data.etts.length == 0) {
                                Ext.getCmp('mark').disable();
                                Ext.getCmp('recheck').disable();
                                Ext.getCmp('finish').disable();
                                Ext.getCmp('updateMemo').disable();
                            }

                            getChkStatus();
                        } else {
                            Ext.Msg.alert('失敗', '發生例外錯誤');
                        }
                    },
                    failure: function (response, options) {

                    }
                });
            },
            failure: function (response, options) {

            }
        });

        
    });
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '篩選設定',
                id: 'btnAddFilter',
                name: 'btnAddFilter',
                handler: function () {
                    filterWindow.show();
                }
            },
            {
                text: '設定重盤品項',
                id: 'mark',
                name: 'mark',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請點選要設定重盤之品項');
                        return;
                    }

                    mark(selection);
                }
            },
            {
                text: '開始重盤',
                id: 'recheck',
                name: 'recheck',
                disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('開始重盤', '是否送出盤點單？', function (btn, text) {
                        if (btn === 'yes') {
                            recheck();
                        }
                    }
                    );
                }
            },
            {
                text: '全藥局盤點結束',
                id: 'finish',
                name: 'finish',
                disabled: true,
                handler: function () {
                    var chkym = T1Query.getForm().findField('P1').rawValue;
                    Ext.MessageBox.confirm('全藥局盤點結束', '是否完成' + chkym + '盤點？', function (btn, text) {
                        if (btn === 'yes') {
                            finish();
                        }
                    }
                    );

                }
            },
            {
                text: '儲存備註',
                id: 'updateMemo',
                name: 'updateMemo',
                disabled: true,
                handler: function () {

                    var tempData = T1Grid.getStore().data.items;
                    var list = [];
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            list.push(tempData[i].data);
                        }
                    }

                    if (list.length == 0) {
                        Ext.Msg.alert('提醒', '無修改的資料');
                        return;
                    }

                    Ext.Ajax.request({
                        url: '/api/CE0024/UpdateMemo',
                        method: reqVal_p,
                        params: { item_string: Ext.util.JSON.encode(list) },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('備註修改成功');
                                T1Load(false);
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                }
            },
        ]
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        id:'T1Grid',
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
            //{
            //    xtype: 'toolbar',
            //    dock: 'top',
            //    items: [{
            //        xtype: 'button',
            //        text: '移除',
            //        id: 'T1Remove',
            //        handler: function () {
            //            var selection = T1Grid.getSelection();
            //            if (selection.length == 0) {
            //                Ext.Msg.alert('提醒', '請點選要移除之資料');
            //                return;
            //            }

            //            remove(selection);
            //        }
            //    }]
            //}
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
                width: 90,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.STATUS == 'R') {
                        return '<span style="color:blue">' + value + '</span>';
                    } else {
                        return '<span>' + value + '</span>';
                    }
                }
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 150,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.STATUS == 'R') {
                        return '<span style="color:blue">' + value + '</span>';
                    } else {
                        return '<span>' + value + '</span>';
                    }
                }
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 150,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.STATUS == 'R') {
                        return '<span style="color:blue">' + value + '</span>';
                    } else {
                        return '<span>' + value + '</span>';
                    }
                }
            },
            {
                text: "劑量單位",
                dataIndex: 'BASE_UNIT',
                width: 80,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.STATUS == 'R') {
                        return '<span style="color:blue">' + value + '</span>';
                    } else {
                        return '<span>' + value + '</span>';
                    }
                }
            },
            {
                text: "電腦量",
                dataIndex: 'STORE_QTY',
                width: 80,
                align: 'right',
                style: 'text-align:left',
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.STATUS == 'R') {
                        return '<span style="color:blue">' + value + '</span>';
                    } else {
                        return '<span>' + value + '</span>';
                    }
                }
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                width: 80,
                align: 'right',
                style: 'text-align:left',
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.STATUS == 'R') {
                        return '<span style="color:blue">' + value + '</span>';
                    } else {
                        return '<span>' + value + '</span>';
                    }
                }
            },
            {
                text: "誤差量",
                dataIndex: 'GAP_T',
                width: 80,
                align: 'right',
                style: 'text-align:left',
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.STATUS == 'R') {
                        return '<span style="color:blue">' + value + '</span>';
                    } else {
                        return '<span>' + value + '</span>';
                    }
                }
            },
            {
                text: "消耗量",
                dataIndex: 'APL_OUTQTY',
                width: 80,
                align: 'right',
                style: 'text-align:left',
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.STATUS == 'R') {
                        return '<span style="color:blue">' + value + '</span>';
                    } else {
                        return '<span>' + value + '</span>';
                    }
                }
            },
            {
                text: "誤差百分比",
                dataIndex: 'MISS_PER',
                align: 'right',
                style: 'text-align:left',
                width: 120,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.STATUS == 'R') {
                        return '<span style="color:blue">' + value + '</span>';
                    } else {
                        return '<span>' + value + '</span>';
                    }
                }
            },
            {
                text: "前次重盤品項",
                dataIndex: 'IS_RECHECK',
                width: 90,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.STATUS == 'R') {
                        return '<span style="color:blue">' + value + '</span>';
                    } else {
                        return '<span>' + value + '</span>';
                    }
                }
            },
            {
                text: '<span style="color:red">備註</span>',
                dataIndex: 'MEMO',
                width: 120,
                editor: {
                    xtype: 'textfield',
                    maxLength: 50,
                    enforceMaxLength:true
                },
            },
            {
                header: "",
                flex: 1

            }
        ],
        listeners: {
            beforeedit: function (editor, e) {
                if (isEditable(T1Query.getForm().findField('P1').rawValue) == false) {
                    return false;
                }
                if (isFinish) {
                    return false;
                }

                return true;
            }
        },
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ],
        viewConfig: {
            listeners: {
                itemkeydown: function (grid, rec, item, idx, e) {
                    if (e.keyCode == 38) { //上
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx - 1, editPlugin.context.colIdx);
                    } else if (e.keyCode == 40) { //下
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx + 1, editPlugin.context.colIdx);
                    }

                    //if (e.keyCode == 13) { //enter
                    //    T3Load();
                    //}

                    //alert('The press key is' + e.getKey());
                }
            }
        },
    });
    function mark(data) {
        var list = [];
        for (var i = 0; i < data.length; i++) {
            var item = {
                //CHK_YM: getChkYm(T1Query.getForm().findField('P0').getValue()),
                CHK_NO: data[i].data.CHK_NO,
                MMCODE: data[i].data.MMCODE
            };
            list.push(item);
        }

        Ext.Ajax.request({
            url: '/api/CE0024/Mark',
            method: reqVal_p,
            params: {
                item_string: Ext.util.JSON.encode(list),
                chk_no1: list[0].CHK_NO
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('設定重盤項目成功');

                    T1Grid.getSelectionModel().deselectAll();
                    getChkStatus();


                    T1Load();

                } else {
                    Ext.Msg.alert('失敗', '發生例外錯誤');
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function recheck() {
        var datas = T1Grid.getStore();
        
        Ext.Ajax.request({
            url: '/api/CE0024/Recheck',
            method: reqVal_p,
            params: {
                chk_no1: datas.data.items[0].data.CHK_NO
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('送出盤點單成功');

                    T1Grid.getSelectionModel().deselectAll();
                    getChkStatus();


                    T1Load();

                } else {
                    Ext.Msg.alert('失敗', data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function finish() {
        
        var myMask = new Ext.LoadMask(Ext.getCmp('T1Grid'), { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.timeout = 300000;
        Ext.Ajax.setTimeout(300000);
        Ext.Ajax.request({
            url: '/api/CE0024/Finish',
            method: reqVal_p,
            params: {
                chk_ym: T1Query.getForm().findField('P1').rawValue
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('已完成盤點');
                    myMask.hide();
                    T1Load();
                } else {
                    myMask.hide();
                    Ext.Msg.alert('失敗', '發生例外錯誤');
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function getChkStatus() {
        // 非當月不可修改
        if (isEditable(T1Query.getForm().findField('P1').rawValue)) {
            Ext.getCmp('recheck').disable();
            Ext.getCmp('mark').disable();
            Ext.getCmp('finish').disable();
            Ext.getCmp('updateMemo').disable();
        }

        Ext.Ajax.request({
            url: '/api/CE0024/GetG2ChkStatus',
            method: reqVal_p,
            params: {
                chk_ym: T1Query.getForm().findField('P1').rawValue
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var allNullChkno = true;
                    for (var i = 0; i < data.etts.length; i++) {
                        
                        if (data.etts[i].CHK_NO != null) {
                            allNullChkno = false;
                        }
                        if (data.etts[i].CHK_WH_NO == T1Query.getForm().findField('P0').getValue()) {
                            sCHK_STATUS = Number(data.etts[i].CHK_STATUS);
                            if (Number(data.etts[i].CHK_STATUS) != 3) {
                                Ext.getCmp('recheck').disable();
                                Ext.getCmp('mark').disable();
                                Ext.getCmp('updateMemo').disable();
                            } else {
                                Ext.getCmp('recheck').enable();
                                Ext.getCmp('mark').enable();
                                Ext.getCmp('updateMemo').enable();
                            }

                            var status = '';
                            if (data.etts[i].CHK_LEVEL == '1') {
                                status += '初盤 ';
                            } else if (data.etts[i].CHK_LEVEL == '2') {
                                status += '複盤 ';
                            } else {
                                status += '未開單 ';
                            }

                            if (data.etts[i].CHK_STATUS == '0') {
                                status += '準備';
                            } else if (data.etts[i].CHK_STATUS == '1') {
                                status += '盤中';
                            } else if (data.etts[i].CHK_STATUS == '2') {
                                status += '調整';
                            } else if (data.etts[i].CHK_STATUS == '3') {
                                status += '鎖單';
                            } else if (data.etts[i].CHK_STATUS == '4') {
                                status += '重盤';
                            }

                            T1Query.getForm().findField('P2').setValue(status);
                        }
                    }
                    
                    Ext.getCmp('finish').disable();
                    var count = 0;
                    for (var i = 0; i < data.etts.length; i++) {
                        
                        //// 正式上線時復原
                        //if (Number(data.etts[i].CHK_STATUS) != 3) {
                        //    count++;
                        //}
                        if (data.etts[i].CHK_NO != null) {
                            if (Number(data.etts[i].CHK_STATUS) != 3) {
                                count++;
                               // Ext.getCmp('finish').disable();
                            }
                        }
                    }
                    if (count == 0) {
                        Ext.getCmp('finish').enable();
                    }
                    if (allNullChkno) {
                        Ext.getCmp('finish').disable();
                        Ext.getCmp('updateMemo').disable();
                        Ext.getCmp('recheck').disable();
                    }
                } else {
                    Ext.Msg.alert('失敗', '發生例外錯誤');
                }
                // 已完成不可修改
                if (isFinish) {
                    Ext.getCmp('recheck').disable();
                    Ext.getCmp('mark').disable();
                    Ext.getCmp('finish').disable();
                    Ext.getCmp('updateMemo').disable();
                }
            },
            failure: function (response, options) {

            }
        });
    }
    // getChkStatus();

    function getIfFihish() {
        Ext.Ajax.request({
            url: '/api/CE0024/GetIfFinish',
            method: reqVal_p,
            params: {
                chk_ym: T1Query.getForm().findField('P1').rawValue
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    isFinish = (data.msg == "Y");
                    if (isFinish) {
                        T1Query.getForm().findField('P3').show();
                    } else {
                        T1Query.getForm().findField('P3').hide();
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getIfFihish();

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
                        height: '100%',
                        split: true,
                        items: [T1Grid]
                    }
                ]
            }
        ]
    });

    getAploutqtyDateRange();

      //#region filter window
    var T1Form = Ext.widget({
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
                            name: 'ORI_RECHECK_ONLY',
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '單價 ≧',
                            name: 'F_U_PRICE',
                            enforceMaxLength: false,
                            labelSeparator: '',
                            maxLength: 14,
                            hideTrigger: true,
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '誤差數量  ≧',
                            name: 'F_NUMBER',
                            enforceMaxLength: false,
                            maxLength: 14,
                            labelSeparator: '',
                            hideTrigger: true,
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
                            maxLength: 14,
                            labelSeparator: '',
                            hideTrigger: true,
                            value: 0
                        },
                        {
                            xtype: 'radiogroup',
                            name: 'RECHECK_ONLY',
                            fieldLabel: '僅重盤品項',
                            items: [
                                { boxLabel: '否', width: 50, name: 'RECHECK_ONLY', inputValue: 'false', checked: true },
                                { boxLabel: '是', width: 50, name: 'RECHECK_ONLY', inputValue: 'true' }
                            ]
                        },
                    ]
                },

            ]
        }]
    });
    T1Form.getForm().findField('RECHECK_ONLY').setValue({ RECHECK_ONLY: 'false' });

    var filterWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T1Form],
        resizable: false,
        draggable: false,
        closable: false,
        title: "篩選條件設定",
        buttons: [
            {
                text: '確定',
                handler: function () {

                    var f = T1Form.getForm();
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
                    f.findField('ORI_RECHECK_ONLY').setValue(f.findField('RECHECK_ONLY').getValue());

                    T1Store.getProxy().setExtraParam("F_U_PRICE", f.findField('F_U_PRICE').getValue());
                    T1Store.getProxy().setExtraParam("F_NUMBER", f.findField('F_NUMBER').getValue());
                    T1Store.getProxy().setExtraParam("F_AMOUNT", f.findField('F_AMOUNT').getValue());
                    T1Store.getProxy().setExtraParam("MISS_PER", f.findField('MISS_PER').getValue());
                    T1Store.getProxy().setExtraParam("RECHECK_ONLY", f.findField('RECHECK_ONLY').getValue());

                    T1Load(true);

                    filterWindow.hide();
                }
            },
            {
                text: '取消',
                handler: function () {
                    var f = T1Form.getForm();
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
                    f.findField('RECHECK_ONLY').setValue(f.findField('ORI_RECHECK_ONLY').getValue());

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
    //#endregion

    //#region chkResultWindow window
    var T11Store = viewModel.getStore('ChkResult');

    function T11Load(clearMsg) {

        if (clearMsg) {
            msglabel('');
        }
        
        T11Store.getProxy().setExtraParam("chk_ym", T1Query.getForm().findField('P1').rawValue);
        T11Store.getProxy().setExtraParam("wh_no", T11Query.getForm().findField('T11P2').getValue());
        T11Store.getProxy().setExtraParam("content_type", T11Query.getForm().findField('T11P1').getValue()['T11P1']);
        T11Tool.moveFirst();
    }
    var T12Store = Ext.create('Ext.data.Store', {
        model: 'WEBAPP.model.CE0027',
        pageSize: 20,
        remoteSort: true,
        listeners: {
            beforeload: function (store, options) {
                
                var np = {
                    chk_ym: T1Query.getForm().findField('P1').rawValue,
                    wh_no: T11Query.getForm().findField('T11P2').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (successful) {
                    setT11Form(store.data.items[0].data);
                }
                if (!successful) {
                    Ext.Msg.alert('失敗', store.proxy.reader.rawData.msg);
                }
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0024/GetChkCounts',
            timeout:120000,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T12Load() {
        T12Store.loadPage();
    }

    var T11Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: 250
        },
        items: [{
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP11',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點年月',
                            name: 'T11P0',
                            id: 'T11P0',
                            labelWidth: mLabelWidth,
                            
                        },
                        {
                            xtype: 'combo',
                            store: whnoQueryStore1,
                            name: 'T11P2',
                            id: 'T11P2',
                            fieldLabel: '庫房代碼',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            fieldCls: 'required',
                            anyMatch: true,
                            allowBlank: false,
                            typeAhead: true,
                            forceSelection: true,
                            triggerAction: 'all',
                            multiSelect: false,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                        },
                        {
                            xtype: 'radiogroup',
                            name: 'T11P1',
                            fieldLabel: '顯示內容',
                            labelAlign: 'right',
                            items: [
                                { boxLabel: '全部', width: 60, name: 'T11P1', inputValue: '0', checked: true },
                                { boxLabel: '盤盈', width: 60, name: 'T11P1', inputValue: '1' },
                                { boxLabel: '盤虧', width: 60, name: 'T11P1', inputValue: '2' }
                            ],
                            listeners: {
                                change: function (field, newValue, oldValue) {

                                }
                            }
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {

                                T11Load();
                                T12Load();

                            }
                        },
            //            {
            //                xtype: 'button',
            //    text: '匯出',
            //    id: 'btnExport',
            //    name: 'btnExport',
            //    handler: function () {
            //        var p = new Array();
            //        p.push({ name: 'chk_ym', value: T1Query.getForm().findField('P1').rawValue });
            //        p.push({ name: 'wh_no', value: T11Query.getForm().findField('T11P2').getValue() });
            //        p.push({ name: 'content_type', value: T11Query.getForm().findField('T11P1').getValue()['T11P1'] });

            //        PostForm('/api/CE0024/Excel', p); 
            //    }
            //},
                    ]
                }
            ]
        }]
    });
    var T11Tool = Ext.create('Ext.PagingToolbar', {
        store: T11Store,
        border: false,
        displayInfo: true,
        plain: true,
        buttons: [
            {
                text: '匯出',
                id: 'btnExport',
                name: 'btnExport',
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'chk_ym', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'wh_no', value: T11Query.getForm().findField('T11P2').getValue() });
                    p.push({ name: 'content_type', value: T11Query.getForm().findField('T11P1').getValue()['T11P1'] });

                    PostForm('/api/CE0024/Excel', p); 
                }
            },
        ]
    });
    var T11Grid = Ext.create('Ext.grid.Panel', {
        store: T11Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T11Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T11Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 220
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 220
            },
            {
                text: "庫存量",
                dataIndex: 'STORE_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 80
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 70
            },
            {
                text: "誤差量",
                dataIndex: 'GAP_T',
                align: 'right',
                style: 'text-align:left',
                width: 70
            },
            {
                text: "移動平均價",
                dataIndex: 'AVG_PRICE',
                align: 'right',
                style: 'text-align:left',
                width: 90
            },
            {
                text: "誤差金額",
                dataIndex: 'DIFF_COST',
                align: 'right',
                style: 'text-align:left',
                width: 70
            },
            {
                text: "誤差百分比",
                dataIndex: 'DIFF_P',
                align: 'right',
                style: 'text-align:left',
                width: 90
            },
            {
                text: "消耗量",
                dataIndex: 'APL_OUTQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70
            },
            {
                text: "備註",
                dataIndex: 'MEMO',
                width: 200,
                flex: 1
            }
        ]
    });

    var T12Form = Ext.widget({
        xtype: 'form',
        layout: {
            type: 'table',
            columns: 2
        },
        frame: false,
        title: '',
        bodyPadding: '5 5',
        border: false,
        defaultType: 'displayfield',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90,
            labelAlign: "right"
        },
        items: [
            {
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 3,
                    tableAttrs: {
                        style: {
                            width: '100%'
                        }
                    }
                },
                items: [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤總品項現品量總金額',
                        labelWidth: 130,
                        name: 'TOT1',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤盈品項現品量總金額',
                        labelWidth: 130,
                        name: 'P_TOT1',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤虧品項現品量總金額',
                        labelWidth: 130,
                        name: 'N_TOT1',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤總品項電腦量總金額',
                        labelWidth: 130,
                        name: 'TOT2',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤盈品項電腦量總金額',
                        labelWidth: 130,
                        name: 'P_TOT2',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤虧品項電腦量總金額',
                        labelWidth: 130,
                        name: 'N_TOT2',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤總品項誤差百分比',
                        labelWidth: 130,
                        name: 'TOT3',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤盈品項誤差百分比',
                        labelWidth: 130,
                        name: 'P_TOT3',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤虧品項誤差百分比',
                        labelWidth: 130,
                        name: 'N_TOT3',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤總品項誤差總金額',
                        labelWidth: 130,
                        name: 'TOT4',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤盈品項誤差總金額',
                        labelWidth: 130,
                        name: 'P_TOT4',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤虧品項誤差總金額',
                        labelWidth: 130,
                        name: 'N_TOT4',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤總品項當季耗總金額',
                        labelWidth: 130,
                        name: 'TOT5',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤盈品項當季耗總金額',
                        labelWidth: 130,
                        name: 'P_TOT5',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤虧品項當季耗總金額',
                        labelWidth: 130,
                        name: 'N_TOT5',
                    }
                ]
            }]
    });
    function setT11Form(data) {
        var f = T12Form.getForm();
        f.findField('TOT1').setValue(data.TOT1);
        f.findField('TOT2').setValue(data.TOT2);
        f.findField('TOT3').setValue(data.TOT3);
        f.findField('TOT4').setValue(data.TOT4);
        f.findField('TOT5').setValue(data.TOT5);
        f.findField('P_TOT1').setValue(data.P_TOT1);
        f.findField('P_TOT2').setValue(data.P_TOT2);
        f.findField('P_TOT3').setValue(data.P_TOT3);
        f.findField('P_TOT4').setValue(data.P_TOT4);
        f.findField('P_TOT5').setValue(data.P_TOT5);
        f.findField('N_TOT1').setValue(data.N_TOT1);
        f.findField('N_TOT2').setValue(data.N_TOT2);
        f.findField('N_TOT3').setValue(data.N_TOT3);
        f.findField('N_TOT4').setValue(data.N_TOT4);
        f.findField('N_TOT5').setValue(data.N_TOT5);
    }

    var chkResultWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [{
                    itemId: 't11Grid',
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    title: '',
                    border: false,
                    height: windowHeight - 180 - 40,
                    items: [T11Grid]
                },
                {
                    itemId: 'form',
                    region: 'south',
                    collapsible: false,
                    floatable: true,
                    split: true,
                    height: 180,
                    collapsed: false,
                    border: false,
                    layout: {
                        type: 'fit',
                        padding: 5,
                        align: 'stretch'
                    },
                    items: [T12Form]
                }]
            },
            ],
        resizable: false,
        draggable: false,
        closable: false,
        title: "盤盈虧現況查詢",
        buttons: [
            {
                text: '關閉',
                handler: function () {

                    chkResultWindow.hide();
                }
            }
        ],
        width: windowWidth,
        height: windowHeight
    });
    chkResultWindow.hide();
    //#endregion

    Ext.on('resize', function () {
        windowHeight = $(window).height();
        windowWidth = $(window).width();
        chkResultWindow.setHeight(windowHeight);
        chkResultWindow.setWidth(windowWidth);
        T11Grid.setHeight(windowHeight - 180 - 40);
    });
});