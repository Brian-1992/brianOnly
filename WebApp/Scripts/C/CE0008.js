Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var WhnoComboGet = '../../../api/CE0002/GetWhnoCombo';
    var T1Name = '三盤管理作業';
    var reportUrl = '/Report/C/CE0002.aspx';
    var reportUrlPH1S = '/Report/C/CE0002_1.aspx';
    var reportUrlWard = '/Report/C/CE0002_2.aspx';
    var reportMultiChknoUrl = '/Report/C/CE0016.aspx';
    var reportMultiMmcodeUrl = '/Report/C/CE0016_1.aspx';
    var reportMultiMmcodeWardUrl = '/Report/C/CE0016_2.aspx';

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

    var viewModel = Ext.create('WEBAPP.store.CE0008VM');

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
    //setComboData();

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

        Ext.getCmp('btnWardChangeUid').hide();
        var whItem = whnoQueryStore.findRecord('WH_NO', p0);
        if (whItem.get('WH_GRADE') == '1') {
            return;
        } else {
            Ext.getCmp('btnWardChangeUid').show();
        }

        if (p0 != '') {
            checkNeedDetailAdd(p0);
        }
    }


    function isEditable(chk_ym) {
        if (chk_ym.substring(0, 5) != todayDateString) {
            return false;
        } else {
            return true;
        }
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
                    width:'100%',
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
                            allowBlank: false,
                            fieldCls: 'required',
                            typeAhead: true,
                            filedCls:'required',
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
                            value: userName
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                
                                msglabel('訊息區:');
                                var f = T1Query.getForm();
                                if (!f.findField('P0').getValue()) {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
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
                        }, {
                            xtype: 'button',
                            text: '新增開單後庫存異動品項',
                            id: 'addNotExists',
                            handler: function () {
                                var f = this.up('form').getForm();
                                Ext.MessageBox.confirm('', '品項新增至本庫房<span style="color:red">開立</span>或<span style="color:red">盤中</span>之盤點單<br/>是否確定？', function (btn, text) {
                                    if (btn === 'yes') {
                                        addNotExists(T1Query.getForm().findField('P0').getValue());
                                    }
                                }
                                );
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
                                    text: '三盤數量輸入(電腦版)',
                                    handler: function () {
                                        parent.link2('/Form/Index/CE0018', ' 三盤數量輸入(電腦版)(CE0018)', true);
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
                    
                    Ext.MessageBox.confirm('刪除', '刪除盤點單，也會將該盤點單內的目前準備旳盤點品項刪除<br/>是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            deleteMaster(list);
                        }
                    }
                    );
                }
            },
            {
                itemId: 'MultiPrint',
                id: 'btnMultiPrint',
                text: "多筆列印",
                disabled: true,
                handler: function () {

                    var selection = T1Grid.getSelection();

                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要列印的盤點單');
                        return;
                    }

                    for (var i = 0; i < selection.length; i++) {
                        if (Number(selection[i].data.CHK_STATUS) < 1) {
                            Ext.Msg.alert('提醒', '盤點單號：' + selection[i].data.CHK_NO + ' 狀態為準備中，不可列印');
                            return;
                        }
                    }

                    multiPrintWindow.show();
                }
            },
            {
                text: '單筆列印',
                id: 'btnPrint',
                name: 'btnPrint',
                disabled: true,
                handler: function () {
                    if (Number(T1LastRec.data.CHK_STATUS) < 1) {
                        Ext.Msg.alert('提醒', '盤點單號：' + T1LastRec.data.CHK_NO + ' 狀態為準備中，不可列印');
                        return;
                    }
                    T24Form.getForm().findField('print_order').setValue('store_loc, mmcode, chk_uid');
                    printWindow.show();
                }
            },
            {
                text: '修改盤點人員',
                id: 'btnWardChangeUid',
                name: 'btnWardChangeUid',
                hidden: true,
                disabled: true,
                handler: function () {
                    if (Number(T1LastRec.data.CHK_STATUS) > 1 || isNaN(Number(T1LastRec.data.CHK_STATUS))) {
                        Ext.Msg.alert('提醒', '盤點單號：' + T1LastRec.data.CHK_NO + ' 不可修改盤點人員');
                        return;
                    }

                    T6Load();
                    var title = T1LastRec.data.CHK_NO + ' ' + T1LastRec.data.WH_NAME + ' ' + T1LastRec.data.CHK_YM + ' ' + T1LastRec.data.CHK_PERIOD_NAME;
                    wardChangeUidWindow.setTitle('修改盤點人員 ' + title);

                    wardChangeUidWindow.show();
                }
            }
        ]
    });
    function deleteMaster(list) {
        Ext.Ajax.request({
            url: '/api/CE0002/DeleteMaster',
            method: reqVal_p,
            params: { ITEM_STRING: Ext.util.JSON.encode(list), preLevel: '2' },
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
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
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
                text: "物料分類",
                dataIndex: 'CHK_CLASS_NAME',
                width: 80
            },
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
            selectionchange: function (model, records) {
                T1Rec = records.length;
                

                Ext.getCmp('btnPrint').disable();
                Ext.getCmp('btnMultiPrint').disable();
                Ext.getCmp('btnWardChangeUid').disable();
                Ext.getCmp('btnDelete').disable();

                if (records.length > 0) {
                    T1LastRec = Ext.clone(records[0]);

                    if (isEditable(T1Query.getForm().findField('P1').rawValue)) {
                        Ext.getCmp('btnDelete').enable();
                    } else {
                        Ext.getCmp('btnDelete').disable();
                    }
                }

                if (records.length == 1) {
                    Ext.getCmp('btnPrint').enable();
                    Ext.getCmp('btnMultiPrint').disable();
                    if (T1LastRec.data.CHK_STATUS == '1') {
                        if (isEditable(T1LastRec.data.CHK_YM)) {
                            Ext.getCmp('btnWardChangeUid').enable();
                        }

                    }
                    return;
                    return;
                }

                if (records.length > 1) {
                    Ext.getCmp('btnPrint').disable();
                    Ext.getCmp('btnMultiPrint').enable();
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

                var f = T21Query.getForm();
                f.findField('T21P0').setValue(record.data.WH_NAME);
                f.findField('T21P1').setValue(record.data.CHK_YM);
                f.findField('T21P2').setValue(record.data.CHK_PERIOD_NAME);
                f.findField('T21P3').setValue(record.data.CHK_TYPE_NAME);
                f.findField('T21P4').setValue(record.data.CHK_STATUS_NAME);
                f.findField('T21P5').setValue(record.data.WH_KIND_NAME);
                f.findField('T21P6').setValue(record.data.CHK_NO);
                f.findField('T21P7').setValue(record.data.CHK_KEEPER);

                if (Number(T1LastRec.data.CHK_STATUS) > 0 || isNaN(Number(T1LastRec.data.CHK_STATUS))) {
                    T21Grid.getColumns()[0].hide();
                    T22Grid.getColumns()[0].hide();
                    Ext.getCmp('btnRemoveFrInclude').disable();
                    Ext.getCmp('btnSave').disable();
                    Ext.getCmp('btnCreateSheet').disable();
                    Ext.getCmp('btnAddToInclude').disable();
                    Ext.getCmp('btnPrint').enable();

                    Ext.getCmp('btnAddItem').disable();

                    T21Grid.setHeight(windowHeight - 90);
                    T22Grid.setHeight(0);
                } else {
                    T21Grid.getColumns()[0].show();
                    T22Grid.getColumns()[0].show();
                    Ext.getCmp('btnAddItem').enable();

                    Ext.getCmp('btnRemoveFrInclude').enable();
                    Ext.getCmp('btnSave').enable();
                    Ext.getCmp('btnCreateSheet').enable();
                    Ext.getCmp('btnAddToInclude').enable();
                    Ext.getCmp('btnPrint').disable();

                    T21Grid.setHeight(windowHeight / 2 - 61);
                    T22Grid.setHeight(windowHeight / 2 - 61);
                }

                var f = T22Form.getForm();
                f.findField('F_U_PRICE').setValue(0);
                if (T1LastRec.data.CHK_WH_NO == '560000') {
                    f.findField('F_NUMBER').setValue(1);
                } else {
                    f.findField('F_NUMBER').setValue(0);
                }
                f.findField('F_AMOUNT').setValue(0);
                f.findField('ORI_F_U_PRICE').setValue(0);
                if (T1LastRec.data.CHK_WH_NO == '560000') {
                    f.findField('ORI_F_NUMBER').setValue(1);
                } else {
                    f.findField('ORI_F_NUMBER').setValue(0);
                }
                f.findField('ORI_F_AMOUNT').setValue(0);

                var loadT22 = (Number(T1LastRec.data.CHK_STATUS) == 0);

                T21Load(loadT22);
                T23Load();

                

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
                
                detailWindow.setX(($(window).width() - detailWindow.getWidth()) / 2);

                Ext.getCmp('btnAddItem').hide();
                if (T1LastRec.data.CHK_WH_KIND == '1' && Number(T1LastRec.data.CHK_WH_GRADE) > 1 &&
                    T1LastRec.data.CHK_KEEPER == T1LastRec.data.CHK_WH_NO) {
                    Ext.getCmp('btnAddItem').show();
                }

                detailWindow.show();
            },
        }
    });


    //#region ------ 盤點項目編輯 ------
    var T21Store = viewModel.getStore('DetailInclude');
    T21Store.on('beforeload', function (store, options) {

        T21Store.getProxy().setExtraParam('windowNewOpen', windowNewOpen == true ? 'Y' : 'N');

        windowNewOpen = false;
    });
    function T21Load(isLoadT22) {
        loadT22 = isLoadT22;

        T21Store.removeAll();
        T22Store.removeAll();

        T21Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T21Store.getProxy().setExtraParam("chk_status", T1LastRec.data.CHK_STATUS);

        T21Tool.moveFirst();
    }
    T21Store.on('load', function (store, options) {
        if (loadT22) {
            T22Load(true);
        }
    });
    var T22Store = viewModel.getStore('DetailExclude');
    T22Store.on('beforeload', function (store, options) {
        
        var temp = T21Grid.getStore().data.items;
        var list = [];
        for (var i = 0; i < temp.length; i++) {
            list.push(temp[i].data);
        }
        T22Store.getProxy().setExtraParam("list", Ext.util.JSON.encode(list));
    });
    function T22Load(isMoveFirst) {
        var f = T22Form.getForm();
        
        T22Store.getProxy().setExtraParam("chk_no1", T1LastRec.data.CHK_NO1);
        T22Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T22Store.getProxy().setExtraParam("wh_no", T1LastRec.data.CHK_WH_NO);
        T22Store.getProxy().setExtraParam("F_U_PRICE", f.findField('F_U_PRICE').getValue());
        T22Store.getProxy().setExtraParam("F_NUMBER", f.findField('F_NUMBER').getValue());
        T22Store.getProxy().setExtraParam("F_AMOUNT", f.findField('F_AMOUNT').getValue());
        
        T22Store.getProxy().setExtraParam("F_MMCODE", f.findField('F_MMCODE').getValue());

        if (isMoveFirst) {
            T22Tool.moveFirst();
        } else {
            T22Store.load({
                params: {
                    start: 0
                }
            });
        }
    }
    var T23Store = viewModel.getStore('PickUsers');
    function T23Load() {

        T23Store.getProxy().setExtraParam("wh_no", T1LastRec.data.CHK_WH_NO);

        T23Tool.moveFirst();
    }
    var T21Query = Ext.widget({
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
                    id: 'PanelP21',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            name: 'T21P0',
                            id: 'T21P0',
                            labelWidth: 60,
                            width: 180,
                            fieldLabel: '庫房代碼',
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點年月',
                            name: 'T21P1',
                            id: 'T21P1',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點期',
                            name: 'T21P2',
                            id: 'T21P2',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點類別',
                            name: 'T21P3',
                            id: 'T21P3',
                            labelWidth: 60,
                            width: 150,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '狀態',
                            name: 'T21P4',
                            id: 'T21P4',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP22',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點單號',
                            name: 'T21P6',
                            id: 'T21P6',
                            labelWidth: 60,
                            width: 180,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            name: 'T21P5',
                            id: 'T21P5',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            fieldLabel: '庫房分類',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '負責人',
                            name: 'T21P7',
                            id: 'T21P7',
                            labelWidth: 60,
                            width: 150,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        }
                    ]
                }
            ]
        }]
    });
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
                            name: 'ORI_F_MMCODE',
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
                            maxLength: 14,
                            labelSeparator: '',
                            hideTrigger: true,
                            value: 0
                        },
                        {
                            xtype: 'textfield',
                            fieldLabel: '院內碼',
                            name: 'F_MMCODE',
                            enforceMaxLength: false,
                            maxLength: 14,
                        }
                    ]
                },
                //{
                //    xtype: 'panel',
                //    id: 'PanelP24',
                //    border: false,
                //    layout: 'hbox',
                //    items: [
                //        {
                //            xtype: 'button',
                //            text: '確定',
                //            handler: function () {

                //                var f = T22Form.getForm();
                //                if (f.findField('F_U_PRICE').getValue() == null) {
                //                    f.findField('F_U_PRICE').setValue(0);
                //                }
                //                if (f.findField('F_NUMBER').getValue() == null) {
                //                    f.findField('F_NUMBER').setValue(0);
                //                }
                //                if (f.findField('F_AMOUNT').getValue() == null) {
                //                    f.findField('F_AMOUNT').setValue(0);
                //                }

                //                f.findField('ORI_F_U_PRICE').setValue(f.findField('F_U_PRICE').getValue());
                //                f.findField('ORI_F_NUMBER').setValue(f.findField('F_NUMBER').getValue());
                //                f.findField('ORI_F_AMOUNT').setValue(f.findField('F_AMOUNT').getValue());

                //                T22Store.getProxy().setExtraParam("F_U_PRICE", f.findField('F_U_PRICE').getValue());
                //                T22Store.getProxy().setExtraParam("F_NUMBER", f.findField('F_NUMBER').getValue());
                //                T22Store.getProxy().setExtraParam("F_AMOUNT", f.findField('F_AMOUNT').getValue());

                //                T22Load();

                //                filterWindow.hide();
                //            }
                //        },
                //        {
                //            xtype: 'button',
                //            text: '取消',
                //            handler: function () {
                //                var f = T22Form.getForm();
                //                if (f.findField('F_U_PRICE').getValue() == null) {
                //                    f.findField('F_U_PRICE').setValue(0);
                //                }
                //                if (f.findField('F_NUMBER').getValue() == null) {
                //                    f.findField('F_NUMBER').setValue(0);
                //                }
                //                if (f.findField('F_AMOUNT').getValue() == null) {
                //                    f.findField('F_AMOUNT').setValue(0);
                //                }

                //                f.findField('F_U_PRICE').setValue(f.findField('ORI_F_U_PRICE').getValue());
                //                f.findField('F_NUMBER').setValue(f.findField('ORI_F_NUMBER').getValue());
                //                f.findField('F_AMOUNT').setValue(f.findField('ORI_F_AMOUNT').getValue());

                //                filterWindow.hide();
                //            }
                //        }
                //    ]
                //}
            ]
        }]
    });

    var T23Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        id: 'T23Form',
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
                    id: 'PanelP24',
                    border: false,
                    layout: 'vbox',
                    items: [
                        {
                            xtype: 'radiogroup',
                            name: 'IS_DISTRI',
                            fieldLabel: '是否分配人員',
                            items: [
                                { boxLabel: '否', width: 50, name: 'IS_DISTRI', inputValue: 'false', checked: true },
                                { boxLabel: '是', width: 50, name: 'IS_DISTRI', inputValue: 'true' }
                            ], listeners: {
                                change: function (field, newValue, oldValue) {
                                    Ext.getCmp('ORDERWAY_radio').hide();
                                    if (newValue['IS_DISTRI'] == 'true') {
                                        T23Form.getForm().findField('ORDERWAY').setValue({ ORDERWAY: 'STORE_LOC' });
                                        Ext.getCmp('ORDERWAY_radio').show();
                                    }
                                }
                            }

                        },
                        {
                            xtype: 'radiogroup',
                            name: 'ORDERWAY',
                            id:'ORDERWAY_radio',
                            fieldLabel: '排序方式',
                            items: [
                                { boxLabel: '儲位', width: 50, name: 'ORDERWAY', inputValue: 'STORE_LOC', checked: true },
                                { boxLabel: '院內碼', width: 80, name: 'ORDERWAY', inputValue: 'MMCODE'},
                                { boxLabel: '電腦量', width: 80, name: 'ORDERWAY', inputValue: 'INV_QTY' }
                            ]
                        },
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

                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        var data = selection[i].data;
                        data.CHK_NO = T1LastRec.data.CHK_NO;
                        list.push(data);
                    }

                    Ext.Ajax.request({
                        url: '/api/CE0002/DeleteDetailTemp',
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
                text: '暫存',
                id: 'btnSave',
                name: 'btnSave',
                handler: function () {
                    
                    var items = T21Grid.getStore().data.items;
                    detailSave(items);
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

                    
                    if (Number(T1LastRec.data.CHK_WH_GRADE) >= 2) {
                        Ext.MessageBox.confirm('分配人員', '是否重新分配人員?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.getCmp('T23Form').hide();
                                pickUserWindow.show();
                                return;
                            } else {
                                Ext.MessageBox.confirm('產生盤點單', '確定產生盤點單?', function (btn, text) {
                                    if (btn === 'yes') {
                                        createSheetWard([]);
                                    }
                                });
                            }
                        }
                        );
                        return;
                    }

                    if (T1LastRec.data.CHK_WH_KIND == "0") {
                        T23Grid.getSelectionModel().deselectAll();
                        Ext.MessageBox.confirm('指派人員', '是否重新指派人員?<br>若不重新指派，則由前次盤點人員繼續盤點', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.getCmp('T23Form').show();
                                
                                T23Form.getForm().findField('ORDERWAY').setValue({ ORDERWAY: 'store_loc' });
                                T23Form.getForm().findField('IS_DISTRI').setValue({ IS_DISTRI: 'false' });
                                Ext.getCmp('ORDERWAY_radio').hide();
                                pickUserWindow.show();
                                return;
                            } else {
                                Ext.MessageBox.confirm('產生盤點單', '確定產生盤點單?', function (btn, text) {
                                    if (btn === 'yes') {
                                        createSheet(T21Grid.getStore().data.items, [], T1LastRec.data.CHK_WH_KIND, '', 'false', 'false');
                                    }
                                }
                                );
                            }
                        }
                        );
                    } else {

                        Ext.MessageBox.confirm('產生盤點單', '確定產生盤點單?', function (btn, text) {
                            if (btn === 'yes') {
                                createSheet(T21Grid.getStore().data.items, [], T1LastRec.data.CHK_WH_KIND, '', 'false', 'false');
                            }
                        }
                        );
                    }
                }
            },
            {
                text: '加入非初盤複盤項目',
                id: 'btnAddItem',
                name: 'btnAddItem',
                hidden: true,
                handler: function () {
                    
                    T41Form.getForm().findField('MMCODE').setValue('');
                    T41Load();
                    addItemWindow.show();
                }
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
                id: 'btnAddToInclude',
                name: 'btnAddToInclude',
                handler: function () {
                    var selection = T22Grid.getSelection();

                    //if (T1LastRec.data.CHK_WH_KIND == "1") {
                    //    for (var i = 0; i < selection.length; i++) {
                    //        if (selection[i].data.CHK_UID == null || selection[i].data.CHK_UID.trim() == "") {
                    //            Ext.Msg.alert('提醒', '所選品項未設定管理人員，請先設定');
                    //            return;
                    //        }
                    //    }
                    //}
                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        var data = selection[i].data;
                        data.CHK_NO = T1LastRec.data.CHK_NO;
                        list.push(data);
                    }

                    Ext.Ajax.request({
                        url: '/api/CE0002/InsertDetailTemp',
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
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        selType: 'checkboxmodel',
        //multiSelect: true,
        //viewConfig: {
        //    plugins: {
        //        ptype: 'gridviewdragdrop',
        //        dragGroup: 'firstGridDDGroup',
        //        dropGroup: 'secondGridDDGroup'
        //    },
        //    listeners: {
        //        //drop: function (node, data, dropRec, dropPosition) {
        //        //    
        //        //    var dropOn = dropRec ? ' ' + dropPosition + ' '
        //        //        + dropRec.get('name') : ' on empty view';
        //        //    var test = T21Grid.getStore();
        //        //}
        //        beforedrop: function(node, data, overModel, dropPosition, dropHandlers, eOpts){

        //        }
        //    }
        //},
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T21Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
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
                text: "開單時電腦量",
                dataIndex: 'INV_QTY',
                //width: 60,
                align: 'right',
                style: 'text-align:left',
                width: 80,
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "儲位 - 量",
                dataIndex: 'STORE_LOC_NAME',
                width: 120,
            },
            {
                text: "盤點人員",
                dataIndex: 'CHK_UID_NAME',
                width: 150,
            },
            //{
            //    text: "狀態",
            //    dataIndex: 'STATUS_INI_NAME',
            //    width: 90,
            //}
        ],
    });
    var T22Grid = Ext.create('Ext.grid.Panel', {
        store: T22Store,
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        selType: 'checkboxmodel',
        //multiSelect: true,
        //viewConfig: {
        //    plugins: {
        //        ptype: 'gridviewdragdrop',
        //        dragGroup: 'secondGridDDGroup',
        //        dropGroup: 'firstGridDDGroup'
        //    },
        //    listeners: {
        //        drop: function (node, data, dropRec, dropPosition) {
        //            var dropOn = dropRec ? ' ' + dropPosition + ' '
        //                + dropRec.get('name') : ' on empty view';
        //        }
        //    }
        //},
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
                xtype: 'rownumberer'
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
                dataIndex: 'STORE_QTYC',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "盤差",
                dataIndex: 'QTY_DIFF',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "誤差比",
                dataIndex: 'DIFF_P',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "儲位 - 量",
                dataIndex: 'STORE_LOC_NAME',
                width: 120,
            },
            {
                text: "盤點人員",
                dataIndex: 'CHK_UID_NAME',
                width: 150,
            },
            {
                text: "狀態",
                dataIndex: 'STATUS_INI_NAME',
                width: 90,
            }
        ],
    });
    var T23Grid = Ext.create('Ext.grid.Panel', {
        store: T23Store,
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'SIMPLE',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
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
            url: '/api/CE0008/DetailSave',
            method: reqVal_p,
            params: {
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

    var createSheet = function (detailStore, userStore, wh_kind, orderway, rechoose, is_distri) {
        var list = [];
        for (var i = 0; i < detailStore.length; i++) {
            var item = detailStore[i].data;
            item.CHK_NO = T1LastRec.data.CHK_NO;
            item.CHK_NO1 = T1LastRec.data.CHK_NO1;
            list.push(item);
        }

        var users = [];
        if (wh_kind == "0") {
            for (var i = 0; i < userStore.length; i++) {
                users.push(userStore[i].data);
            }
        }

        var myMask = new Ext.LoadMask(Ext.getCmp('pickUserWindow'), { msg: '處理中...' });
        myMask.show();
        var myMask1 = new Ext.LoadMask(Ext.getCmp('detailWindow'), { msg: '處理中...' });
        myMask1.show();

        Ext.Ajax.request({
            url: '/api/CE0008/CreateSheet',
            method: reqVal_p,
            params: {
                //users: Ext.util.JSON.encode(users),
                //wh_kind: wh_kind,
                chk_no: T1LastRec.data.CHK_NO,
                chk_no1: T1LastRec.data.CHK_NO1,
                chk_wh_grade: T1LastRec.data.CHK_WH_GRADE,
                chk_wh_kind: T1LastRec.data.CHK_WH_KIND,
                users: Ext.util.JSON.encode(users),
                orderway: orderway,
                is_distri: is_distri,
                rechoose:rechoose
            },
            success: function (response) {
                myMask.hide();
                myMask1.hide();
                windowNewOpen = true;
                T21Load(true);
                //T22Load();
                T1Load(false);

                msglabel("產生盤點單成功");
                detailWindow.hide();
                pickUserWindow.hide();

                T21Grid.getColumns()[0].hide();
                T22Grid.getColumns()[0].hide();
                Ext.getCmp('btnRemoveFrInclude').disable();
                Ext.getCmp('btnSave').disable();
                Ext.getCmp('btnCreateSheet').disable();
                Ext.getCmp('btnAddToInclude').disable();

                T21Query.getForm().findField('T21P4').setValue('1 盤中');

            },
            failure: function (response, options) {
                // Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    var createSheetWard = function (userStore) {
        var users = [];
        for (var i = 0; i < userStore.length; i++) {
            users.push(userStore[i].data);
        }

        var myMask = new Ext.LoadMask(Ext.getCmp('pickUserWindow'), { msg: '處理中...' });
        myMask.show();
        var myMask1 = new Ext.LoadMask(Ext.getCmp('detailWindow'), { msg: '處理中...' });
        myMask1.show();

        Ext.Ajax.request({
            url: '/api/CE0008/CreateSheetWard',
            method: reqVal_p,
            params: {
                wh_no: T1LastRec.data.CHK_WH_NO,
                chk_no: T1LastRec.data.CHK_NO,
                chk_no1: T1LastRec.data.CHK_NO1,
                chk_wh_grade: T1LastRec.data.CHK_WH_GRADE,
                chk_wh_kind: T1LastRec.data.CHK_WH_KIND,
                users: Ext.util.JSON.encode(users),
                preLevel: '2'
            },
            success: function (response) {
                myMask.hide();
                myMask1.hide();
                windowNewOpen = true;
                T21Load(true);
                //T22Load();
                T1Load(false);

                msglabel("產生盤點單成功");

                detailWindow.hide();
                pickUserWindow.hide();

                T21Grid.getColumns()[0].hide();
                T22Grid.getColumns()[0].hide();
                Ext.getCmp('btnRemoveFrInclude').disable();
                Ext.getCmp('btnSave').disable();
                Ext.getCmp('btnCreateSheet').disable();
                Ext.getCmp('btnAddToInclude').disable();

                T21Query.getForm().findField('T21P4').setValue('1 盤中');

            },
            failure: function (response, options) {
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
                Ext.Ajax.request({
                    url: '/api/CE0002/DeleteDetailTemps',
                    method: reqVal_p,
                    params: {
                        chk_no: T1LastRec.data.CHK_NO
                    },
                    success: function (response) {
                        //T21Load(true);
                        //T22Load();
                        detailWindow.hide();
                        windowNewOpen = true;

                        //msglabel("暫存明細成功");
                    },
                    failure: function (response, options) {
                        Ext.Msg.alert('失敗', '發生例外錯誤');
                    }
                });
                var f = T22Form.getForm();

                f.findField('F_U_PRICE').setValue(0);
                f.findField('F_NUMBER').setValue(0);
                f.findField('F_AMOUNT').setValue(0);

                f.findField('F_MMCODE').setValue('');
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                detailWindow.setX(($(window).width() - detailWindow.getWidth()) / 2);
                detailWindow.setY(0);
            }
        }
    });
    detailWindow.hide();

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
                    if (f.findField('F_U_PRICE').getValue() == null) {
                        f.findField('F_U_PRICE').setValue(0);
                    }
                    if (f.findField('F_NUMBER').getValue() == null) {
                        f.findField('F_NUMBER').setValue(0);
                    }
                    if (f.findField('F_AMOUNT').getValue() == null) {
                        f.findField('F_AMOUNT').setValue(0);
                    }

                    f.findField('ORI_F_U_PRICE').setValue(f.findField('F_U_PRICE').getValue());
                    f.findField('ORI_F_NUMBER').setValue(f.findField('F_NUMBER').getValue());
                    f.findField('ORI_F_AMOUNT').setValue(f.findField('F_AMOUNT').getValue());
                    f.findField('ORI_F_MMCODE').setValue(f.findField('F_MMCODE').getValue());

                    T22Store.getProxy().setExtraParam("F_U_PRICE", f.findField('F_U_PRICE').getValue());
                    T22Store.getProxy().setExtraParam("F_NUMBER", f.findField('F_NUMBER').getValue());
                    T22Store.getProxy().setExtraParam("F_AMOUNT", f.findField('F_AMOUNT').getValue());
                    T22Store.getProxy().setExtraParam("F_MMCODE", f.findField('F_MMCODE').getValue());

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

                    f.findField('F_U_PRICE').setValue(f.findField('ORI_F_U_PRICE').getValue());
                    f.findField('F_NUMBER').setValue(f.findField('ORI_F_NUMBER').getValue());
                    f.findField('F_AMOUNT').setValue(f.findField('ORI_F_AMOUNT').getValue());

                    f.findField('F_MMCODE').setValue('');


                    filterWindow.hide();
                }
            }

            //{
            //text: '取消',
            //handler: function () {
            //    filterWindow.hide();
            //}
            //}
        ]
    });
    filterWindow.hide();
    //#endregion --------------------------


    //#region ------ 挑選盤點單 ------
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
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '負責人',
                            name: 'T3P2',
                            id: 'T3P2',
                            labelWidth: 60,
                            width: 190,
                            padding: '0 4 0 4',
                            value: userName
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
                text: '加入三盤',
                id: 'btnAddSecond',
                name: 'btnAddSecond',
                handler: function () {
                    var selection = T31Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要加入三盤的盤點單');
                        return;
                    }
                    
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
            url: '/api/CE0008/InsertMaster',
            method: reqVal_p,
            params: { ITEM_STRING: Ext.util.JSON.encode(list) },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('加入三盤成功');
                    T1Load(false);
                    //T31Load();
                    insertListWindow.hide();
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
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
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
                text: "物料分類",
                dataIndex: 'CHK_CLASS_NAME',
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
                insertListWindow.setX(($(window).width() - insertListWindow.getWidth()) / 2);
                insertListWindow.setY(0);
            }
        }
    });
    insertListWindow.hide();
     //#endregion -------------------------

    var pickUserWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id:'pickUserWindow',
        items: [T23Form,
            T23Grid
        ],
        modal: true,
        //items: [T21Grid, T22Grid],
        width: "400px",
        height: 400,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
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

                    //var user = [];
                    //for (var i = 0; i < selection.length; i++) {
                    //    user.push(selection[i].data);
                    //}
                    
                    Ext.MessageBox.confirm('產生盤點單', '確定產生盤點單?', function (btn, text) {
                        if (btn === 'yes') {
                            if (Number(T1LastRec.data.CHK_WH_GRADE) >= 2) {
                                createSheetWard(selection);
                                return;
                            }
                            
                            var orderway = T23Form.getForm().findField('ORDERWAY').getValue()['ORDERWAY'];
                            var is_distri = T23Form.getForm().findField('IS_DISTRI').getValue()['IS_DISTRI'];

                            createSheet(T21Grid.getStore().data.items, selection, T1LastRec.data.CHK_WH_KIND, orderway, 'true', is_distri);
                        }
                    }
                    );
                }
            },{
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
        draggable: false,
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
                    if (T1LastRec.data.CHK_WH_NO == 'PH1S') {
                        showReport(reportUrlPH1S, T1LastRec.data.CHK_NO, "");
                    } else if (T1LastRec.data.CHK_WH_GRADE == '1') {
                        showReport(reportUrl, T1LastRec.data.CHK_NO, "");
                    } else {
                        showReport(reportUrlWard, T1LastRec.data.CHK_NO, "");
                    }
                    printWindow.hide();
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
    function showReport(url, chk_no, chk_nos) {
        if (!win) {

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + url + '?CHK_NO=' + chk_no
                + '&PRINT_ORDER=' + T24Form.getForm().findField('print_order').getValue()
                + '&CHK_NOS=' + chk_nos
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

    //#region 多筆列印
    var T25Form = Ext.widget({
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
                    border: false,
                    layout: 'hbox',
                    width: 250,
                    items: [
                        {
                            xtype: 'radiogroup',
                            name: 'multiPrintType',
                            fieldLabel: '列印格式',
                            items: [
                                { boxLabel: '依院內碼', width: 70, name: 'multiPrintType', inputValue: 'mmcode', checked: true },
                                { boxLabel: '依單號', width: 70, name: 'multiPrintType', inputValue: 'chk_no' },
                            ]
                        },
                    ]
                },
            ]
        }
        ]
    });

    var multiPrintWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            T25Form
        ],
        resizable: false,
        draggable: false,
        closable: false,
        title: "多筆列印",
        buttons: [
            {
                text: '確定',
                handler: function () {

                    var selection = T1Grid.getSelection();

                    var chk_nos = "";
                    for (var i = 0; i < selection.length; i++) {
                        if (i == 0) {
                            chk_nos = selection[i].data.CHK_NO;
                        } else {
                            chk_nos = chk_nos + "," + selection[i].data.CHK_NO;
                        }
                    }

                    if (T25Form.getForm().findField('multiPrintType').getValue()['multiPrintType'] == 'mmcode') {
                        if (selection[0].data.CHK_WH_KIND == 'E' || selection[0].data.CHK_WH_KIND == 'C') {
                            showReport(reportMultiMmcodeUrl, '', chk_nos);
                        } else {
                            showReport(reportMultiMmcodeWardUrl, '', chk_nos);
                        }
                    } else {
                        showReport(reportMultiChknoUrl, "", chk_nos);
                    }

                    multiPrintWindow.hide();
                }
            },
            {
                text: '取消',
                handler: function () {
                    multiPrintWindow.hide();
                }
            }
        ]
    });
    multiPrintWindow.hide();
    //#endregion
   
    //#region 病房科室衛材盤點單新增項目 addItemWindow
    var T41Form = Ext.widget({
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
                    id: 'PanelP41',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '院內碼',
                            name: 'MMCODE',
                            maxLength: 13,
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');
                                var f = T41Form.getForm();

                                T41Load();
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
                    ]
                },
            ]
        }]
    });

    var T41Store = viewModel.getStore('AddItemList');
    function T41Load() {
        //T41Form.getForm().findField('MMCODE').setValue(T41Form.getForm().findField('MMCODE').getValue().toUpperCase())

        T41Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T41Store.getProxy().setExtraParam("chk_no1", T1LastRec.data.CHK_NO1);
        T41Store.getProxy().setExtraParam("wh_no", T1LastRec.data.CHK_WH_NO);
        T41Store.getProxy().setExtraParam("chk_wh_grade", T1LastRec.data.CHK_WH_GRADE);
        T41Store.getProxy().setExtraParam("chk_wh_kind", T1LastRec.data.CHK_WH_KIND);
        T41Store.getProxy().setExtraParam("chk_type", T1LastRec.data.CHK_TYPE);
        T41Store.getProxy().setExtraParam("mmcode", T41Form.getForm().findField('MMCODE').getValue());
        T41Tool.moveFirst();
    }

    var T41Tool = Ext.create('Ext.PagingToolbar', {
        store: T41Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '加入',
                id: 'T41addItem',
                name: 'T41addItem',
                handler: function () {
                    var selection = T41Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請先選擇項目');
                        return;
                    }

                    Ext.MessageBox.confirm('加入項目', '是否確定新增？', function (btn, text) {
                        if (btn === 'yes') {
                            var list = [];

                            for (var i = 0; i < selection.length; i++) {
                                list.push({
                                    CHK_NO: T1LastRec.data.CHK_NO,
                                    WH_NO: T1LastRec.data.CHK_WH_NO,
                                    MMCODE: selection[i].data.MMCODE
                                });
                            }

                            addItem(list);
                        }
                    });
                }
            }
        ]
    });

    var T41Grid = Ext.create('Ext.grid.Panel', {
        store: T41Store,
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
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
                layout: 'fit',
                items: [T41Form]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T41Tool]
            }
        ],
        columns: [
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
                text: "電腦量",
                dataIndex: 'INV_QTY',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                header: "",
                flex: 1
            }

        ],
    });

    function addItem(list) {

        Ext.Ajax.request({
            url: '/api/CE0005/AddItems',
            method: reqVal_p,
            params: { list: Ext.util.JSON.encode(list) },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('新增項目成功');
                    T41Load();
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var addItemWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        id: 'addItemWindow',
        items: [T41Grid],
        resizable: false,
        draggable: false,
        closable: false,
        title: "新增項目",
        buttons: [
            {
                text: '取消',
                handler: function () {
                    
                    windowNewOpen = false;
                    T21Load(true);
                    addItemWindow.hide();
                }
            }
        ]
    });
    addItemWindow.hide();

    //#endregion

    //#region 修改盤點人員 btnWardChangeUid
    var T6Store = viewModel.getStore('CurrentUids');
    function T6Load() {
        //T41Form.getForm().findField('MMCODE').setValue(T41Form.getForm().findField('MMCODE').getValue().toUpperCase())
        var isWard = "Y";
        if (T1LastRec.data.CHK_WH_GRADE == '2' && T1LastRec.data.CHK_WH_KIND == '0') {
            isWard = "N";
        }
        T6Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T6Store.getProxy().setExtraParam("is_ward", isWard);

        T6Tool.moveFirst();
    }

    var T6Tool = Ext.create('Ext.PagingToolbar', {
        store: T6Store,
        border: false,
        plain: true,
        displayInfo: true
    });

    var T6Grid = Ext.create('Ext.grid.Panel', {
        store: T6Store,
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
                items: [T6Tool]
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

    function changeWardUid() {
        var selection = T6Grid.getSelection();
        var users = [];

        for (var i = 0; i < selection.length; i++) {
            users.push({
                WH_CHKUID: selection[i].data.WH_CHKUID,
                HAS_ENTRY: selection[i].data.HAS_ENTRY,
            });
        }
        var store = T6Grid.getStore().data.items;
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
        var myMask = new Ext.LoadMask(Ext.getCmp('wardChangeUidWindow'), { msg: '處理中...' });
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
                    wardChangeUidWindow.hide();
                }
            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }

    var wardChangeUidWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        id: 'wardChangeUidWindow',
        items: [T6Grid],
        resizable: true,
        closable: false,
        title: "新增項目",
        width: 400,
        buttons: [
            {
                text: '確定',
                handler: function () {
                    changeWardUid();
                }
            },
            {
                text: '取消',
                handler: function () {
                    wardChangeUidWindow.hide();
                }
            }
        ],
        listeners: {
            show: function (self, eOpts) {
                wardChangeUidWindow.center();
                wardChangeUidWindow.setWidth(400);
            }
        }
    });
    wardChangeUidWindow.hide();
    //#endregion

    //#region 2021-10-20 針對開單或盤中的品項新增項目
    function checkNeedDetailAdd(wh_no) {
        Ext.getCmp('addNotExists').hide();
        Ext.Ajax.request({
            url: '/api/CE0002/CheckNeedDetailAdd',
            method: reqVal_p,
            params: {
                wh_no: wh_no
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    Ext.getCmp('addNotExists').show();
                } else {
                    Ext.getCmp('addNotExists').hide();
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function addNotExists(wh_no) {
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0002/AddDetailNotExists',
            method: reqVal_p,
            params: {
                wh_no: wh_no
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                myMask.hide();
                T8Store.removeAll();
                var results = data.etts;
                if (results.length > 0) {
                    for (var i = 0; i < results.length; i++) {
                        T8Store.add(results[i]);
                    }
                }

                addDetailWindow.show();

            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }

    var T8Store = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'CHK_TYPE', 'CHK_TYPE_NAME', 'CHK_LEVEL', 'CHK_STATUS', 'CHK_STATUS_NAME',
            'MMCODE_COUNT', 'MMCODE_STRING', 'RESULT']
    });
    var T8Grid = Ext.create('Ext.grid.Panel', {
        store: T8Store,
        id: 'T8Grid',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: [
            {
                text: "盤點類別",
                dataIndex: 'CHK_TYPE',
                width: 80,
                renderer: function (val, meta, record) {
                    var chk_type = record.data.CHK_TYPE;
                    if (chk_type == '0') {
                        return '非庫備';
                    }
                    if (chk_type == '1') {
                        return '庫備';
                    }
                    if (chk_type == '3') {
                        return '小額採購';
                    }
                    return '';
                },
            },
            {
                text: "盤點單號",
                dataIndex: 'CHK_NO',
                width: 120
            },
            {
                text: "盤點階段",
                dataIndex: 'CHK_LEVEL',
                width: 80,
                renderer: function (val, meta, record) {
                    var temp = record.data.CHK_LEVEL;
                    if (temp == '') {
                        return '';
                    }
                    if (temp == '1') {
                        return '初盤';
                    }
                    if (temp == '2') {
                        return '複盤';
                    }
                    if (temp == '3') {
                        return '三盤';
                    }
                    return '';
                },
            },
            {
                text: "盤點單狀態",
                dataIndex: 'CHK_STATUS_NAME',
                width: 90
            },
            {
                text: "新增院內碼數量",
                dataIndex: 'MMCODE_COUNT',
                width: 120
            },
            {
                text: "結果",
                dataIndex: 'RESULT',
                flex: 1
            }
        ],
    });
    var addDetailWindow = Ext.create('Ext.window.Window', {
        id: 'addDetailWindow',
        renderTo: Ext.getBody(),
        items: [T8Grid],
        modal: true,
        width: "800px",
        height: 200,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "新增開單後異動品項",
        buttons: [
            {
                text: '關閉',
                handler: function () {
                    addDetailWindow.hide();
                    checkNeedDetailAdd(T1Query.getForm().findField('P0').getValue());
                    T1Load();
                }
            }],
        listeners: {
            show: function (self, eOpts) {
                addDetailWindow.center();
                addDetailWindow.setWidth(800);
            }
        }
    });
    //#endregion

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

        addItemWindow.setHeight(windowHeight);
        addItemWindow.setWidth(windowWidth);

        if (detailWindow.hidden == false) {
            if (Number(T1LastRec.data.CHK_STATUS) > 0) {
                T21Grid.setHeight(windowHeight - 90);
                T22Grid.setHeight(0);
            }
        }
        pickUserWindow.center();
        insertListWindow.center();
        insertListWindow.setHeight(windowHeight);
        T31Grid.setHeight(windowHeight - 60);
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();

    Ext.getCmp('btnDelete').disable();
});