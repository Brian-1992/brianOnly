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
    var todayDateString = '';
    var set_ym = '';
    var pym = '';
    var nym = '';
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

    var viewModel = Ext.create('WEBAPP.store.CE0015VM');

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;

    // ------ 主畫面 ------

    function chknoExists() {
        Ext.Ajax.request({
            url: '/api/CE0015/ChknoExists',
            method: reqVal_p,
            params: { P0: T1Query.getForm().findField('P0').rawValue },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.etts.length > 0) {
                        // Ext.getCmp('T1Remove').disable();

                        Ext.getCmp('remove').disable();
                        Ext.getCmp('T21add').disable();
                        Ext.getCmp('T22add').disable();
                        Ext.getCmp('T23add').disable();

                        //Ext.getCmp('createSMasts').disable();
                        //Ext.getCmp('createPMasts').disable();
                        Ext.getCmp('P1').disable();
                        //Ext.getCmp('createSMasts').disable();
                        
                    } else {
                        // Ext.getCmp('T1Remove').enable();

                        //
                        //Ext.getCmp('T21Add').enable();
                        //Ext.getCmp('T22Add').enable();
                        //Ext.getCmp('T23Add').enable();

                        Ext.getCmp('remove').enable();
                        Ext.getCmp('T21add').enable();
                        Ext.getCmp('T22add').enable();
                        Ext.getCmp('T23add').enable();

                       // Ext.getCmp('createSMasts').enable();
                        //Ext.getCmp('createPMasts').enable();
                        Ext.getCmp('P1').enable();
                        //Ext.getCmp('createSMasts').enable();
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function chknoSExists() {
        Ext.Ajax.request({
            url: '/api/CE0015/ChknoSExists',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    
                    if (data.etts.length > 0) {
                        Ext.getCmp('createSMasts').disable();
                        Ext.getCmp('createPMasts').disable();
                        Ext.getCmp('P1').disable();

                    } else {
                        Ext.getCmp('createSMasts').enable();
                        Ext.getCmp('createPMasts').enable();
                        Ext.getCmp('P1').enable();
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function getTodayDate() {
        Ext.Ajax.request({
            url: '/api/CE0015/GetSetYm',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var list = data.etts;
                    if (list.length > 0) {
                        var item = list[0];
                        
                        set_ym = item.SET_YM;
                        todayDateString = item.SET_YM;
                        pym = item.PYM;
                        nym = item.NYM;

                        var temp = '產生本月(' + todayDateString + ')所有藥局季盤單';

                        var temp1 = '產生本月(' + todayDateString + ')所有藥局抽盤單';

                        Ext.getCmp('createSMasts').setText(temp);
                        Ext.getCmp('createPMasts').setText(temp1);

                        var temp2 = '預開次月(' + nym + ')盤點單'
                        Ext.getCmp('openNextMonthWindow').setText(temp2);

                        
                        T3Form.getForm().findField('CHK_YM').setValue(nym);

                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    
    function getDateFormat(value) {
        //
        var yyyy = value.getFullYear().toString();
        var m = value.getMonth() + 1;
        var d = value.getDate();
        var mm = m > 9 ? m.toString() : "0" + m.toString();
        var dd = d > 9 ? d.toString() : "0" + d.toString();
        return yyyy + "-" + mm + "-" + dd;
    }

    var T1Store = viewModel.getStore('Include');
    function T1Load() {
        
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').rawValue);
        //T1Tool.moveFirst();
        T1Store.load();
    }

    var T21Store = viewModel.getStore('All');
    function T21Load() {
        T21Store.getProxy().setExtraParam("E_ORDERDCFLAG", T22Form.getForm().findField('E_ORDERDCFLAG').getValue());
        //T21Store.load();
        T21Tool.moveFirst();
    }

    var T22Store = viewModel.getStore('PreviousP');
    function T22Load() {
        
        T22Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').rawValue);

        T22Tool.moveFirst();
        //T22Store.load();
    }

    var T23Store = viewModel.getStore('PreviousS');
    function T23Load() {
        T23Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').rawValue);

        T23Tool.moveFirst();
        //T23Store.load();
    }

    var getToday = function () {
        
        var yyyy = new Date().getFullYear();
        var m = new Date().getMonth() + 1;
        var yyy = (yyyy - 1911).toString();
        var mm = m > 9 ? m.toString() : '0' + m.toString();
        return yyy + mm;
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
                            xtype: 'monthfield',
                            fieldLabel: '盤點年月',
                            name: 'P0',
                            id: 'P0',
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
                            listeners: {
                                change: function (self, newValue, oldValue) {
                                    
                                    //var temp = newValue;
                                    //temp = new Date(temp.setMonth(temp.getMonth() - 1));
                                    //T1Query.getForm().findField('P00').setValue(temp);
                                    //resetFilter();

                                    T21Store.getProxy().setExtraParam({});
                                    T22Store.getProxy().setExtraParam({});
                                    T23Store.getProxy().setExtraParam({});

                                    setP00Value(newValue);
                                    T1Load();
                                    T21Load();
                                    T22Load();
                                    T23Load();
                                    chknoExists();
                                }
                            }
                        },
                        {
                            xtype: 'monthfield',
                            fieldLabel: '盤點年月',
                            name: 'P00',
                            id: 'P00',
                            // enforceMaxLength: true,
                            //maxLength: 5,
                            //minLength: 5,
                            //regexText: '請填入民國年月',
                            //regex: /\d{5,5}/,
                            hidden: true,
                            labelWidth: mLabelWidth,
                            width: 180,
                            padding: '0 4 0 4',
                        },
                        {
                            xtype: 'datefield',
                            fieldLabel: '預計盤點日期',
                            name: 'P1',
                            id: 'P1',
                            labelWidth: 80,
                            width: 180,
                            padding: '0 4 0 20',
                            labelAlign: 'right',
                            minValue: new Date()
                        },
                        {
                            xtype: 'button',
                            text: '',
                            id: 'createSMasts',
                            handler: function () {
                                if (T1Query.getForm().findField('P1').getValue() == null || T1Query.getForm().findField('P1').getValue() == undefined) {
                                    Ext.Msg.alert('提醒', '請選擇預計盤點日期');
                                    return;
                                }
                                if (T1Query.getForm().findField('P1').isValid() == false) {
                                    Ext.Msg.alert('提醒', '日期小於今天或格式錯誤，請重新選擇');
                                    return;
                                }

                                var temp = '是否確定產生' + todayDateString + '藥局季盤單？';
                                Ext.MessageBox.confirm('產生季盤單', temp, function (btn, text) {
                                    if (btn === 'yes') {
                                        myMask.show();
                                        Ext.Ajax.request({
                                            url: '/api/CE0015/CreateSMats',
                                            method: reqVal_p,
                                            params: {
                                                chk_ym: todayDateString,
                                                chk_pre_date: getDateFormat(T1Query.getForm().findField('P1').getValue())
                                            },
                                            success: function (response) {
                                                var data = Ext.decode(response.responseText);
                                                if (data.success) {
                                                    msglabel('季盤單產生成功');
                                                    myMask.hide();
                                                    chknoExists();
                                                    chknoSExists();
                                                } else {
                                                    Ext.Msg.alert('失敗', '發生例外錯誤');
                                                    myMask.hide();
                                                }
                                            },
                                            failure: function (response, options) {
                                                myMask.hide();
                                            }
                                        });
                                    }
                                });
                            }
                        },
                        {
                            xtype: 'button',
                            text: '',
                            id: 'createPMasts',
                            handler: function () {
                                if (T1Query.getForm().findField('P1').getValue() == null || T1Query.getForm().findField('P1').getValue() == undefined) {
                                    Ext.Msg.alert('提醒', '請選擇預計盤點日期');
                                    return;
                                }
                                if (T1Query.getForm().findField('P1').isValid() == false) {
                                    Ext.Msg.alert('提醒', '日期小於今天或格式錯誤，請重新選擇');
                                    return;
                                }

                                var temp = '是否確定產生' + todayDateString + '藥局抽盤單？';
                                Ext.MessageBox.confirm('產生抽盤單', temp, function (btn, text) {
                                    if (btn === 'yes') {
                                        myMask.show();
                                        Ext.Ajax.request({
                                            url: '/api/CE0015/CreatePMats',
                                            method: reqVal_p,
                                            params: {
                                                chk_ym: todayDateString,
                                                chk_pre_date: getDateFormat(T1Query.getForm().findField('P1').getValue())
                                            },
                                            success: function (response) {
                                                var data = Ext.decode(response.responseText);
                                                if (data.success) {
                                                    msglabel('抽盤單產生成功');
                                                    myMask.hide();
                                                    chknoExists();
                                                    chknoSExists();
                                                } else {
                                                    Ext.Msg.alert('失敗', '發生例外錯誤');
                                                    myMask.hide();
                                                }
                                            },
                                            failure: function (response, options) {
                                                myMask.hide();
                                            }
                                        });
                                    }
                                });
                            }
                        },
                        {
                            xtype: 'button',
                            text: '',
                            id: 'openNextMonthWindow',
                            handler: function () {
                                CheckNextMonthExists();
                                nextMonthWindow.show();
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
                text: '移除',
                id: 'remove',
                name: 'remove',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請點選要移除之資料');
                        return;
                    }

                    remove(selection);
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
                xtype: 'rownumberer',
                width: 40
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 90
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 150,
            },
            {
                text: "劑量單位",
                dataIndex: 'BASE_UNIT',
                width: 80
            },
            {
                text: "單價",
                dataIndex: 'M_CONTPRICE',
                width: 120,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "服用類別",
                dataIndex: 'E_TAKEKIND',
                xtype: 'templatecolumn',
                tpl: '{E_TAKEKIND_NAME}',
                width: 80
            },
            {
                header: "",
                flex: 1

            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                T1LastRec = record;
                //T2Load();
            }
        }
    });

    var T21Tool = Ext.create('Ext.PagingToolbar', {
        store: T21Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '加入',
                id: 'T21add',
                name: 'T21add',
                handler: function () {
                    var selection = T21Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請點選要加入之資料');
                        return;
                    }

                    insert(selection);
                }
            },
            ,
            {
                text: '篩選',
                id: 'T21filter',
                name: 'T21filter',
                handler: function () {
                    //var selection = T22Grid.getSelection();
                    //if (selection.length == 0) {
                    //    Ext.Msg.alert('提醒', '請點選要加入之資料');
                    //    return;
                    //}
                    if (T22Form.getForm().findField('WH_NO').getValue() == null) {
                        T22Form.getForm().findField('WH_NO').setValue('');
                    }

                    //insert(selection);
                    filterWindow.show();
                }
            },
        ]
    });
    var T21Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T21Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T21Tool]
            }
        ],
        viewConfig: {
            preserveScrollOnRefresh: true
        },
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer',
                width: 40
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 90
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
                text: "劑量單位",
                dataIndex: 'BASE_UNIT',
                width: 80
            },
            {
                text: "單價",
                dataIndex: 'M_CONTPRICE',
                width: 120,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "服用類別",
                dataIndex: 'E_TAKEKIND',
                xtype: 'templatecolumn',
                tpl: '{E_TAKEKIND_NAME}',
                width: 80
            },
            {
                header: "",
                flex: 1
            }
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
                id: 'T22add',
                name: 'T22add',
                handler: function () {
                    var selection = T22Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請點選要加入之資料');
                        return;
                    }

                    insert(selection);
                }
            },
            {
                text: '篩選',
                id: 'T22filter',
                name: 'T22filter',
                handler: function () {
                    //var selection = T22Grid.getSelection();
                    //if (selection.length == 0) {
                    //    Ext.Msg.alert('提醒', '請點選要加入之資料');
                    //    return;
                    //}
                    if (T22Form.getForm().findField('WH_NO').getValue() == null) {
                        T22Form.getForm().findField('WH_NO').setValue('');
                    }
                    //insert(selection);
                    filterWindow.show();
                }
            },
        ]
    });
    var T22Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T22Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T22Tool]
            }
        ],
        viewConfig: {
            preserveScrollOnRefresh: true
        },
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer',
                width: 40
            },
            {
                text: "庫房",
                dataIndex: 'WH)NO',
                xtype: 'templatecolumn',
                tpl: '{WH_NO} {WH_NAME}',
                width: 120
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 90
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
                text: "劑量單位",
                dataIndex: 'BASE_UNIT',
                width: 80
            },
            {
                text: "單價",
                dataIndex: 'M_CONTPRICE',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "電腦量",
                dataIndex: 'STORE_QTYC',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "盤差",
                dataIndex: 'GAP_T',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "盤盈虧",
                dataIndex: 'DIFF_AMOUNT',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                header: "",
                flex: 1
            }
        ]
    });

    var T23Tool = Ext.create('Ext.PagingToolbar', {
        store: T23Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '加入',
                id: 'T23add',
                name: 'T23add',
                handler: function () {
                    var selection = T23Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請點選要加入之資料');
                        return;
                    }

                    insert(selection);

                }
            },
            ,
            {
                text: '篩選',
                id: 'T23filter',
                name: 'T23filter',
                handler: function () {
                    //var selection = T22Grid.getSelection();
                    //if (selection.length == 0) {
                    //    Ext.Msg.alert('提醒', '請點選要加入之資料');
                    //    return;
                    //}
                    
                    if (T22Form.getForm().findField('WH_NO').getValue() == null) {
                        T22Form.getForm().findField('WH_NO').setValue('');
                    }
                    //insert(selection);
                    filterWindow.show();
                }
            },
        ]
    });
    var T23Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T23Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        viewConfig: {
            preserveScrollOnRefresh: true
        },
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T23Tool]
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
                xtype: 'rownumberer',
                width: 40
            },
            {
                text: "庫房",
                dataIndex: 'WH)NO',
                xtype: 'templatecolumn',
                tpl: '{WH_NO} {WH_NAME}',
                width: 120
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 90
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
                text: "劑量單位",
                dataIndex: 'BASE_UNIT',
                width: 80
            },
            {
                text: "單價",
                dataIndex: 'M_CONTPRICE',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "電腦量",
                dataIndex: 'STORE_QTYC',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "盤差",
                dataIndex: 'GAP_T',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "盤盈虧",
                dataIndex: 'DIFF_AMOUNT',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                header: "",
                flex: 1
            }
        ]
    });

    function getChkYm(chk_ym) {
        
        var yyyy = chk_ym.getFullYear();
        var m = chk_ym.getMonth() + 1;
        var yyy = (yyyy - 1911).toString();
        var mm = m > 9 ? m.toString() : '0' + m.toString();
        return yyy + mm;
    }

    function insert(data) {
        
        var list = [];
        for (var i = 0; i < data.length; i++) {
            var item = {
                CHK_YM: getChkYm(T1Query.getForm().findField('P0').getValue()),
                MMCODE: data[i].data.MMCODE
            };
            list.push(item);
        }

        Ext.Ajax.request({
            url: '/api/CE0015/Insert',
            method: reqVal_p,
            params: {
                ITEM_STRING: Ext.util.JSON.encode(list)
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('加入成功');

                    T21Grid.getSelectionModel().deselectAll();
                    T22Grid.getSelectionModel().deselectAll();
                    T23Grid.getSelectionModel().deselectAll();

                    T1Load();
                } else {
                    Ext.Msg.alert('失敗', '發生例外錯誤');
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function remove(data) {
        var list = [];
        for (var i = 0; i < data.length; i++) {
            var item = {
                CHK_YM: getChkYm(T1Query.getForm().findField('P0').getValue()),
                MMCODE: data[i].data.MMCODE
            };
            list.push(item);
        }

        Ext.Ajax.request({
            url: '/api/CE0015/Delete',
            method: reqVal_p,
            params: {
                ITEM_STRING: Ext.util.JSON.encode(list)
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('移除成功');

                    T21Grid.getSelectionModel().deselectAll();;
                    T22Grid.getSelectionModel().deselectAll();;

                    T1Load();
                } else {
                    Ext.Msg.alert('失敗', '發生例外錯誤');
                }
            },
            failure: function (response, options) {

            }
        });
    }

    //#region filter window

    // 設定狀態
    var drugtypeStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     
            { "VALUE": "", "TEXT": "全部" },
            { "VALUE": "1", "TEXT": "管制藥" },
            { "VALUE": "2", "TEXT": "高價藥" },
            { "VALUE": "3", "TEXT": "研究用藥" },
            { "VALUE": "4", "TEXT": "罕見病藥" },
            { "VALUE": "5", "TEXT": "公費藥" }
        ]
    });
    // 藥局清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "", "TEXT": "所有藥局" },
        ]
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
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

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
                            name: 'ORI_GAP_T',  // 誤差量 
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_GAP_PRICE',  // 誤差金額
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_M_CONTPRICE',    // 進價
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_GAP_P',  // 誤差百分比
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_E_ORDERDCFLAG',  // 停用
                            xtype: 'hidden',
                            value: 0
                        },
                        {
                            name: 'ORI_DRUG_TYPE',  // 篩選條件
                            xtype: 'hidden',
                            value: ''
                        },
                        {
                            name: 'ORI_WH_NO',  // 篩選條件
                            xtype: 'hidden',
                            value: ''
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '誤差量 >',
                            name: 'GAP_T',
                            enforceMaxLength: false,
                            maxLength: 14,
                            labelSeparator: '',
                            hideTrigger: true,
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '誤差金額 >',
                            name: 'GAP_PRICE',
                            enforceMaxLength: false,
                            maxLength: 14,
                            labelSeparator: '',
                            hideTrigger: true,
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '進價 >',
                            name: 'M_CONTPRICE',
                            enforceMaxLength: false,
                            maxLength: 14,
                            labelSeparator: '',
                            hideTrigger: true,
                            value: 0
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '誤差百分比 >',
                            name: 'GAP_P',
                            enforceMaxLength: false,
                            maxLength: 14,
                            labelSeparator: '',
                            hideTrigger: true,
                            value: 0
                        },
                        {
                            xtype: 'radiogroup',
                            name: 'E_ORDERDCFLAG',
                            fieldLabel: '停用',
                            items: [
                                { boxLabel: '否', name: 'E_ORDERDCFLAG', inputValue: 'N', width: 50, checked: true },
                                { boxLabel: '是', name: 'E_ORDERDCFLAG', inputValue: 'Y' }
                            ],
                            listeners: {
                                change: function (field, newValue, oldValue) {
                                    
                                }
                            },
                            //labelWidth: 100,
                            //width: 250
                        },
                        {
                            xtype: 'combo',
                            store: drugtypeStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            fieldLabel: '篩選條件',
                            name: 'DRUG_TYPE',
                            id: 'DRUG_TYPE',
                            enforceMaxLength: true,
                            padding: '0 4 0 4',
                            //width: 150
                            value:''
                        },
                        {
                            xtype: 'combo',
                            store: whnoQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            fieldLabel: '庫房代碼',
                            name: 'WH_NO',
                            id: 'WH_NO',
                            enforceMaxLength: true,
                            padding: '0 4 0 4',
                            //width: 150
                            value: ''
                        },
                    ]
                },

            ]
        }]
    });

    var filterWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T22Form],
        //width: "900px",
        //height: windowHeight,
        resizable: false,
        draggable: false,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        //y: 0,
        title: "篩選條件設定",
        //listeners: {
        //    close: function (panel, eOpts) {
        //        myMask.hide();
        //    }
        //}
        buttons: [
            {
                text: '確定',
                handler: function () {

                    var f = T22Form.getForm();
                    if (f.findField('GAP_T').getValue() == null) {
                        f.findField('GAP_T').setValue(0);
                    }
                    if (f.findField('GAP_PRICE').getValue() == null) {
                        f.findField('GAP_PRICE').setValue(0);
                    }
                    if (f.findField('M_CONTPRICE').getValue() == null) {
                        f.findField('M_CONTPRICE').setValue(0);
                    }
                    if (f.findField('GAP_P').getValue() == null) {
                        f.findField('GAP_P').setValue(0);
                    }

                    f.findField('ORI_GAP_T').setValue(f.findField('GAP_T').getValue());
                    f.findField('ORI_GAP_PRICE').setValue(f.findField('GAP_PRICE').getValue());
                    f.findField('ORI_M_CONTPRICE').setValue(f.findField('M_CONTPRICE').getValue());
                    f.findField('ORI_GAP_P').setValue(f.findField('GAP_P').getValue());
                    f.findField('ORI_E_ORDERDCFLAG').setValue(f.findField('E_ORDERDCFLAG').getValue());
                    f.findField('ORI_E_ORDERDCFLAG').setValue(f.findField('E_ORDERDCFLAG').getValue());
                    f.findField('ORI_DRUG_TYPE').setValue(f.findField('DRUG_TYPE').getValue());
                    f.findField('ORI_WH_NO').setValue(f.findField('WH_NO').getValue());

                    T21Store.getProxy().setExtraParam("M_CONTPRICE", f.findField('M_CONTPRICE').getValue());
                    T21Store.getProxy().setExtraParam("E_ORDERDCFLAG", f.findField('E_ORDERDCFLAG').getValue());
                    T21Store.getProxy().setExtraParam("DRUG_TYPE", f.findField('DRUG_TYPE').getValue());

                    T22Store.getProxy().setExtraParam("GAP_T", f.findField('GAP_T').getValue());
                    T22Store.getProxy().setExtraParam("GAP_PRICE", f.findField('GAP_PRICE').getValue());
                    T22Store.getProxy().setExtraParam("M_CONTPRICE", f.findField('M_CONTPRICE').getValue());
                    T22Store.getProxy().setExtraParam("GAP_P", f.findField('GAP_P').getValue());
                    T22Store.getProxy().setExtraParam("E_ORDERDCFLAG", f.findField('E_ORDERDCFLAG').getValue());
                    T22Store.getProxy().setExtraParam("DRUG_TYPE", f.findField('DRUG_TYPE').getValue());
                    T22Store.getProxy().setExtraParam("WH_NO", f.findField('WH_NO').getValue());

                    T23Store.getProxy().setExtraParam("GAP_T", f.findField('GAP_T').getValue());
                    T23Store.getProxy().setExtraParam("GAP_PRICE", f.findField('GAP_PRICE').getValue());
                    T23Store.getProxy().setExtraParam("M_CONTPRICE", f.findField('M_CONTPRICE').getValue());
                    T23Store.getProxy().setExtraParam("GAP_P", f.findField('GAP_P').getValue());
                    T23Store.getProxy().setExtraParam("E_ORDERDCFLAG", f.findField('E_ORDERDCFLAG').getValue());
                    T23Store.getProxy().setExtraParam("DRUG_TYPE", f.findField('DRUG_TYPE').getValue());
                    T23Store.getProxy().setExtraParam("WH_NO", f.findField('WH_NO').getValue());

                    T21Load();
                    T22Load();
                    T23Load();

                    filterWindow.hide();
                }
            },
            {
                text: '取消',
                handler: function () {
                    var f = T22Form.getForm();
                    if (f.findField('GAP_T').getValue() == null) {
                        f.findField('GAP_T').setValue(0);
                    }
                    if (f.findField('GAP_PRICE').getValue() == null) {
                        f.findField('GAP_PRICE').setValue(0);
                    }
                    if (f.findField('M_CONTPRICE').getValue() == null) {
                        f.findField('M_CONTPRICE').setValue(0);
                    }
                    if (f.findField('GAP_P').getValue() == null) {
                        f.findField('GAP_P').setValue(0);
                    }

                    f.findField('GAP_T').setValue(f.findField('ORI_GAP_T').getValue());
                    f.findField('GAP_PRICE').setValue(f.findField('ORI_GAP_PRICE').getValue());
                    f.findField('M_CONTPRICE').setValue(f.findField('ORI_M_CONTPRICE').getValue());
                    f.findField('GAP_P').setValue(f.findField('ORI_GAP_P').getValue());
                    f.findField('E_ORDERDCFLAG').setValue(f.findField('ORI_E_ORDERDCFLAG').getValue());
                    f.findField('DRUG_TYPE').setValue(f.findField('ORI_DRUG_TYPE').getValue());
                    f.findField('WH_NO').setValue(f.findField('ORI_WH_NO').getValue());

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

    function resetFilter() {
        var f = T22Form.getForm();

        f.findField('GAP_T').setValue(0);
        f.findField('GAP_PRICE').setValue(0);
        f.findField('M_CONTPRICE').setValue(0);
        f.findField('GAP_P').setValue(0);
        f.findField('E_ORDERDCFLAG').setValue('N');
        f.findField('DRUG_TYPE').setValue('');
        f.findField('WH_NO').setValue('');

        f.findField('ORI_GAP_T').setValue(0);
        f.findField('ORI_GAP_PRICE').setValue(0);
        f.findField('ORI_M_CONTPRICE').setValue(0);
        f.findField('ORI_GAP_P').setValue(0);
        f.findField('ORI_E_ORDERDCFLAG').setValue('N');
        f.findField('ORI_DRUG_TYPE').setValue('');
        f.findField('ORI_WH_NO').setValue('');

    }

    //#endregion

    //#region 預開次月
    var T3Form = Ext.widget({
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
                    border: false,
                    layout: 'vbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點年月',
                            name: 'CHK_YM',
                        },
                        {
                            xtype: 'datefield',
                            fieldLabel: '預計盤點日期',
                            name: 'CHK_PRE_DATE',
                            labelWidth: 80,
                            width: 180,
                            padding: '0 4 0 20',
                            labelAlign: 'right',
                            minValue: new Date()
                        },
                        {
                            xtype: 'radiogroup',
                            name: 'CHK_PERIOD',
                            fieldLabel: '盤點類別',
                            items: [
                                { boxLabel: '抽盤', name: 'CHK_PERIOD', inputValue: 'P', width: 50, checked: true },
                                { boxLabel: '季盤', name: 'CHK_PERIOD', inputValue: 'S' }
                            ],
                            listeners: {
                                change: function (field, newValue, oldValue) {

                                }
                            },
                            //labelWidth: 100,
                            //width: 250
                        },
                    ]
                },

            ]
        }]
    });

    function CheckNextMonthExists() {
        Ext.Ajax.request({
            url: '/api/CE0015/ChknoExists',
            method: reqVal_p,
            params: { P0: T3Form.getForm().findField('CHK_YM').getValue() },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.etts.length > 0) {
                        // Ext.getCmp('T1Remove').disable();

                        Ext.getCmp('createNextMonth').disable();
                        //Ext.getCmp('createSMasts').disable();

                    } else {
                        // Ext.getCmp('T1Remove').enable();

                        //
                        //Ext.getCmp('T21Add').enable();
                        //Ext.getCmp('T22Add').enable();
                        //Ext.getCmp('T23Add').enable();

                        Ext.getCmp('createNextMonth').enable();
                       
                        //Ext.getCmp('createSMasts').enable();
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var nextMonthWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T3Form],
        //width: "900px",
        //height: windowHeight,
        resizable: false,
        draggable: false,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        //y: 0,
        title: "預開次月",
        //listeners: {
        //    close: function (panel, eOpts) {
        //        myMask.hide();
        //    }
        //}
        buttons: [
            {
                text: '開立',
                id:'createNextMonth',
                handler: function () {
                    if (T3Form.getForm().findField('CHK_PRE_DATE').getValue() == null || T3Form.getForm().findField('CHK_PRE_DATE').getValue() == undefined) {
                        Ext.Msg.alert('提醒', '請選擇預計盤點日期');
                        return;
                    }
                    if (T3Form.getForm().findField('CHK_PRE_DATE').isValid() == false) {
                        Ext.Msg.alert('提醒', '日期小於今天或格式錯誤，請重新選擇');
                        return;
                    }
                    var period = T3Form.getForm().findField('CHK_PERIOD').getValue()['CHK_PERIOD'];
                    
                    url = '/api/CE0015/Create' + period + 'Mats';

                    var windowMask = new Ext.LoadMask(nextMonthWindow, { msg: '處理中...' });

                    var temp = '是否確定產生' + nym + '藥局' + (period =='S' ?'季' :'抽')+'盤單？';
                    Ext.MessageBox.confirm('產生' + (period == 'S' ? '季' : '抽')+'盤單', temp, function (btn, text) {
                        if (btn === 'yes') {
                            myMask.show();
                            windowMask.show();
                            Ext.Ajax.request({
                                url: url,
                                method: reqVal_p,
                                params: {
                                    chk_ym: nym,
                                    chk_pre_date: getDateFormat(T3Form.getForm().findField('CHK_PRE_DATE').getValue())
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel((period == 'S' ? '季' : '抽')+'盤單產生成功');
                                        myMask.hide();
                                        windowMask.hide();
                                        chknoExists();
                                        chknoSExists();
                                    } else {
                                        Ext.Msg.alert('失敗', '發生例外錯誤');
                                        myMask.hide();
                                        windowMask.hide();
                                    }
                                    nextMonthWindow.hide();
                                },
                                failure: function (response, options) {
                                    myMask.hide();
                                    windowMask.hide();
                                    nextMonthWindow.hide();
                                }
                            });
                        }
                    });
                }
            },
            {
                text: '取消',
                handler: function () {
                    T3Form.getForm().reset();
                    T3Form.getForm().findField('CHK_YM').setValue(nym);
                    nextMonthWindow.hide();
                }
            }
            //{
            //text: '關閉',
            //handler: function () {
            //    filterWindow.hide();
            //}
            //}
        ],
        listeners: {
            show: function (self, eOpts) {
                
                nextMonthWindow.center();
            }
        }
    });
    nextMonthWindow.hide();
    //#endregion

    // --------------------

    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        //enableTabScroll: false,
        layout: 'fit',
        //autoScroll: false,
        //frame:true,
        defaults: {
            layout: 'fit',
            split: true
        },
        items: [{
            title: '未選定盤點清單',
            id: 'tab1',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                region: 'center',
                layout: 'fit',
                split: true,
                collapsible: false,
                border: false,
                height: '50%',
                items: [T21Grid]
            },]
        },
        //{
        //    title: '上期盤點清單',
        //    id: 'tab2',
        //    layout: 'border',
        //    padding: 0,
        //    split: true,
        //    items: [{
        //        region: 'center',
        //        layout: 'fit',
        //        split: true,
        //        collapsible: false,
        //        border: false,
        //        height: '50%',
        //        items: [T22Grid]
        //    }
        {
            title: '上期抽盤清單',
            id: 'tab2',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                region: 'center',
                layout: 'fit',
                split: true,
                collapsible: false,
                border: false,
                height: '50%',
                items: [T22Grid]
            }]
        }, {
            title: '上期季盤清單',
            id: 'tab3',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                region: 'center',
                layout: 'fit',
                split: true,
                collapsible: false,
                border: false,
                height: '50%',
                items: [T23Grid]
            }
            ]
        },
        ],

        listeners: {
            'tabchange': function (tabPanel, tab) {

                //if (tab.id == 'tab1') {
                //    if (T1Query.getForm().findField('WH_ID_CODE').getValue()) {
                //        T1Store.load({
                //            params: {
                //                start: 0
                //            }
                //        });
                //    }
                //} else {
                //    if (T1aQuery.getForm().findField('USER_ID_CODE').getValue()) {
                //        T1aStore.load({
                //            params: {
                //                start: 0
                //            }
                //        });
                //    }
                //}
            }
        }
    });

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
                        itemId: 'tabs',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '50%',
                        split: true,
                        items: [TATabs]
                    }
                ]
            }
        ]
    });

    function setP00Value(p1Value) {
        
        var temp = p1Value;
        temp = new Date(temp.setMonth(temp.getMonth() - 1));
        T1Query.getForm().findField('P00').setValue(temp);
    }

    setP00Value(T1Query.getForm().findField('P0').getValue());

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();

    T1Load();
    T21Load();
    T22Load();
    T23Load();
    chknoExists();
    chknoSExists();
    getTodayDate();
});